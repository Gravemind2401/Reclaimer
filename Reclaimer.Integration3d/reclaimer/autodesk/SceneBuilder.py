from typing import cast
from typing import Dict, Tuple, List
from functools import reduce
import operator

import pymxs
from pymxs import runtime as rt

from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *
from .Utils import *

__all__ = [
    'create_scene'
]

MeshContext = Tuple[Scene, Model, Mesh, rt.Editable_Mesh]

MX_UNITS = 100.0 # 1 max unit = 100mm?

UNIT_SCALE: float = 1.0
OPTIONS: ImportOptions = ImportOptions()

def create_scene(scene: Scene, options: ImportOptions = None):
    global UNIT_SCALE, OPTIONS
    UNIT_SCALE = scene.unit_scale / MX_UNITS
    # OPTIONS = options

    print(f'scene name: {scene.name}')
    print(f'scene scale: {scene.unit_scale}')

    for model in scene.model_pool:
        builder = ModelBuilder(scene, model)
        if OPTIONS.IMPORT_BONES and model.bones:
            print(f'creating {model.name}/skeleton')
            builder.create_bones()
        if OPTIONS.IMPORT_MESHES and model.meshes:
            print(f'creating {model.name}/meshes')
            builder.create_meshes()
        if OPTIONS.IMPORT_MARKERS and model.markers:
            print(f'creating {model.name}/markers')
            builder.create_markers()

def _convert_transform_units(transform: Matrix4x4, bone_mode: bool = False) -> rt.Matrix3:
    ''' Converts a transform from model units to max units '''
    if not bone_mode:
        return toMatrix3(transform) * rt.scaleMatrix(rt.Point3(UNIT_SCALE, UNIT_SCALE, UNIT_SCALE))

    # for bones we want to keep the scale component at 1x, but still need to convert the translation component
    m = toMatrix3(transform)
    return rt.preRotate(rt.transMatrix(m.translationPart * UNIT_SCALE), m.rotationPart)


class ModelBuilder:
    _root_layer: rt.MixinInterface
    _region_layers: Dict[int, rt.MixinInterface]
    _scene: Scene
    _model: Model
    _instances: Dict[Tuple[int, int], rt.Mesh]
    _maxbones = List[rt.BoneGeometry]

    def __init__(self, scene: Scene, model: Model):
        self._root_layer = self._create_layer(OPTIONS.model_name(model))
        self._region_layers = dict()
        self._scene = scene
        self._model = model
        self._instances = dict()

    def _create_layer(self, name: str, key: int = None) -> rt.MixinInterface:
        if key != None:
            name = f'{self._root_layer.name}::{name}'
        layer = rt.LayerManager.newLayerFromName(name) # TODO: enforce unique model names
        if key != None:
            self._region_layers[key] = layer
            layer.setParent(self._root_layer)
        return layer

    def _get_bone_transforms(self) -> List[rt.Matrix3]:
        result = []
        for bone in self._model.bones:
            lineage = self._model.get_bone_lineage(bone)
            transforms = [_convert_transform_units(x.transform, True) for x in reversed(lineage)]
            result.append(reduce(operator.mul, transforms))
        return result

    def create_bones(self):
        scene, model = self._scene, self._model

        BONE_SIZE = 0.03 * UNIT_SCALE * OPTIONS.BONE_SCALE
        TAIL_VECTOR = rt.Point3(BONE_SIZE, 0.0, 0.0)

        bone_layer = self._create_layer('__bones__', -1)
        bone_transforms = self._get_bone_transforms()

        maxbones = self._maxbones = []
        for i, b in enumerate(model.bones):
            maxbone = rt.BoneSys.createBone(rt.Point3(0, 0, 0), TAIL_VECTOR, rt.Point3(0, 0, 1))
            maxbone.setBoneEnable(False, 0)
            maxbone.name = OPTIONS.bone_name(b)
            maxbone.height = maxbone.width = maxbone.length = BONE_SIZE
            maxbones.append(maxbone)
            bone_layer.addnode(maxbone)

            children = model.get_bone_children(b)
            if children:
                size = max(rt.length(toPoint3(b.transform[3])) for b in children)
                maxbone.length = size * UNIT_SCALE

            maxbone.taper = 70 if children else 50
            maxbone.transform = bone_transforms[i]

            if b.parent_index >= 0:
                maxbone.parent = maxbones[b.parent_index]

    def create_markers(self):
        MARKER_SIZE = 0.01 * UNIT_SCALE * OPTIONS.MARKER_SCALE

        marker_layer = None
        bone_transforms = self._get_bone_transforms()

        for marker in self._model.markers:
            for i, instance in enumerate(marker.instances):
                marker_obj = rt.Sphere(radius = MARKER_SIZE)
                marker_obj.name = OPTIONS.marker_name(marker, i)

                # put the marker in the appropriate layer based on region/permutation
                if instance.region_index >= 0 and instance.region_index < 255:
                    self._region_layers[instance.region_index].addnode(marker_obj)
                else:
                    if not marker_layer:
                        marker_layer = self._create_layer('__markers__', -2)
                    marker_layer.addnode(marker_obj)

                world_transform = rt.preRotate(rt.transMatrix(toPoint3(instance.position) * UNIT_SCALE), toQuat(instance.rotation))

                if instance.bone_index >= 0 and self._model.bones:
                    world_transform *= bone_transforms[instance.bone_index]
                    if OPTIONS.IMPORT_BONES:
                        marker_obj.parent = self._maxbones[instance.bone_index]

                marker_obj.renderable = False
                marker_obj.transform = world_transform

    def create_meshes(self):
        mesh_count = 0
        for i, r in enumerate(self._model.regions):
            region_layer = self._create_layer(OPTIONS.region_name(r), i)
            for j, p in enumerate(r.permutations):
                print(f'creating mesh {mesh_count:03d}: {self._model.name}/{r.name}/{p.name} [{i:02d}/{j:02d}]')
                self._build_mesh(region_layer, r, p)
                mesh_count += 1

    def _build_mesh(self, layer: rt.MixinInterface, region: ModelRegion, permutation: ModelPermutation):
        scene, model = self._scene, self._model

        WORLD_TRANSFORM = _convert_transform_units(permutation.transform)

        for mesh_index in range(permutation.mesh_index, permutation.mesh_index + permutation.mesh_count):
            MESH_NAME = OPTIONS.permutation_name(region, permutation, mesh_index)
            SELF_TRANSFORM = toMatrix3(model.meshes[mesh_index].vertex_transform) * WORLD_TRANSFORM
            INSTANCE_KEY = (mesh_index, -1) # TODO: second element reserved for submesh index if mesh splitting enabled

            if INSTANCE_KEY in self._instances.keys():
                source = self._instances.get(INSTANCE_KEY)
                # methods with byref params return a tuple of (return_value, byref1, byref2, ...)
                _, newNodes = rt.MaxOps.cloneNodes(source, cloneType = rt.Name('instance'), newNodes = pymxs.byref(None))
                copy = cast(rt.Mesh, newNodes[0])
                copy.name = MESH_NAME
                copy.transform = SELF_TRANSFORM
                layer.addnode(copy)
                continue

            mesh = model.meshes[mesh_index]
            index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]
            vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

            # note 3dsMax uses 1-based indices for triangles, vertices etc

            positions = list(toPoint3(v) for v in vertex_buffer.position_channels[0])
            faces = list(toPoint3(t) + 1 for t in index_buffer.get_triangles(mesh))

            mesh_obj = cast(rt.Editable_Mesh, rt.Mesh(vertices=positions, faces=faces))
            mesh_obj.name = MESH_NAME
            mesh_obj.transform = SELF_TRANSFORM
            layer.addnode(mesh_obj)
            self._instances[INSTANCE_KEY] = mesh_obj

            mc: MeshContext = (scene, model, mesh, mesh_obj)
            self._build_normals(mc)

    def _build_normals(self, mc: MeshContext):
        scene, model, mesh, mesh_obj = mc
        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

        if not (OPTIONS.IMPORT_NORMALS and vertex_buffer.normal_channels):
            return

        normals = list(toPoint3(v) for v in vertex_buffer.normal_channels[0])
        for i, normal in enumerate(normals):
            # note: prior to 2015, this was only temporary
            rt.setNormal(mesh_obj, i + 1, normal)
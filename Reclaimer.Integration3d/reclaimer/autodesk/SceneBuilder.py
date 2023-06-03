from typing import cast
from typing import Dict, Tuple, List
from functools import reduce
import operator

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
            print('creating skeleton')
            builder.create_bones()
        if OPTIONS.IMPORT_MESHES and model.meshes:
            print('creating meshes')
            builder.create_meshes()
        # if OPTIONS.IMPORT_MARKERS and model.markers:
        #     print('creating markers')
        #     builder.create_markers()

def _convert_transform_units(transform: Matrix4x4, bone_mode: bool = False) -> rt.Matrix3:
    ''' Converts a transform from model units to max units '''
    if not bone_mode:
        return toMatrix3(transform) * rt.scaleMatrix(rt.Point3(UNIT_SCALE, UNIT_SCALE, UNIT_SCALE))

    # for bones we want to keep the scale component at 1x, but still need to convert the translation component
    m = toMatrix3(transform)
    return rt.preRotate(rt.transMatrix(m.translationPart * UNIT_SCALE), m.rotationPart)


class ModelBuilder:
    _scene: Scene
    _model: Model
    _instances: Dict[Tuple[int, int], rt.Mesh]

    def __init__(self, scene: Scene, model: Model):
        self._scene = scene
        self._model = model
        self._instances = dict()

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

        bone_transforms = self._get_bone_transforms()

        maxbones = []
        for i, b in enumerate(model.bones):
            maxbone = rt.BoneSys.createBone(rt.Point3(0, 0, 0), TAIL_VECTOR, rt.Point3(0, 0, 1))
            maxbone.setBoneEnable(False, 0)
            maxbone.name = OPTIONS.bone_name(b)
            maxbone.height = maxbone.width = maxbone.length = BONE_SIZE
            maxbones.append(maxbone)

            children = model.get_bone_children(b)
            if children:
                size = max(rt.length(toPoint3(b.transform[3])) for b in children)
                maxbone.length = size * UNIT_SCALE

            maxbone.taper = 70 if children else 50
            maxbone.transform = bone_transforms[i]

            if b.parent_index >= 0:
                maxbone.parent = maxbones[b.parent_index]

    def create_meshes(self):
        model = self._model

        for r in model.regions:
            # TODO: some kind of object hierarchy equivalent of blender's collections
            for p in r.permutations:
                self._build_mesh(r, p)

    def _build_mesh(self, region: ModelRegion, permutation: ModelPermutation):
        scene, model = self._scene, self._model

        world_transform = _convert_transform_units(permutation.transform)

        for mesh_index in range(permutation.mesh_index, permutation.mesh_index + permutation.mesh_count):
            INSTANCE_KEY = (mesh_index, -1) # TODO: second element reserved for submesh index if mesh splitting enabled

            mesh = model.meshes[mesh_index]
            index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]
            vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

            # note 3dsMax uses 1-based triangle indices!

            positions = list(toPoint3(v) for v in vertex_buffer.position_channels[0])
            normals = list(toPoint3(v) for v in vertex_buffer.normal_channels[0])
            faces = list(toPoint3(t) + 1 for t in index_buffer.get_triangles(mesh))

            mesh_obj = cast(rt.Editable_Mesh, rt.Mesh(vertices=positions, faces=faces))
            mesh_obj.name = OPTIONS.permutation_name(region, permutation, mesh_index)

            mesh_obj.transform = toMatrix3(mesh.vertex_transform)
            mesh_obj.transform *= world_transform

            self._instances[INSTANCE_KEY] = mesh_obj

import bpy
import bmesh
import itertools
import operator
from typing import cast
from typing import Dict, Tuple, List
from mathutils import Vector, Matrix, Quaternion
from bpy.types import Context, Collection, Armature, EditBone, Object
from functools import reduce

from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *
from .Utils import *

__all__ = [
    'create_scene'
]

BL_UNITS: float = 1000.0 # 1 blender unit = 1000mm

UNIT_SCALE: float = 1.0
OPTIONS: ImportOptions = ImportOptions()

def create_scene(context: Context, scene: Scene, options: ImportOptions = None):
    global UNIT_SCALE, OPTIONS
    UNIT_SCALE = scene.unit_scale / BL_UNITS
    # OPTIONS = options

    print(f'scene name: {scene.name}')
    print(f'scene scale: {scene.unit_scale}')

    for model in scene.model_pool:
        print(f'creating model: {model.name}...')
        builder = ModelBuilder(context, scene, model)
        if OPTIONS.IMPORT_BONES and model.bones:
            print('creating armature')
            builder.create_bones()
        if OPTIONS.IMPORT_MESHES and model.meshes:
            print('creating meshes')
            builder.create_meshes()
        if OPTIONS.IMPORT_MARKERS and model.markers:
            print('creating markers')
            builder.create_markers()

def _convert_transform_units(transform: Matrix4x4, bone_mode: bool = False) -> Matrix:
    ''' Converts a transform from model units to blender units '''
    if not bone_mode:
        return Matrix.Scale(UNIT_SCALE, 4) @ Matrix(transform).transposed()

    # for bones we want to keep the scale component at 1x, but still need to convert the translation component
    m = Matrix(transform).transposed()
    translation, rotation, scale = m.decompose()
    return Matrix.Translation(translation * UNIT_SCALE) @ rotation.to_matrix().to_4x4()


class ModelBuilder:
    _context: Context
    _root_collection: Collection
    _region_collections: Dict[int, Collection]
    _scene: Scene
    _model: Model
    _armature_obj: Object
    _instances: Dict[Tuple[int, int], Object]

    def __init__(self, context: Context, scene: Scene, model: Model):
        self._context = context
        self._root_collection = self._create_collection(OPTIONS.model_name(model))
        self._region_collections = dict()
        self._scene = scene
        self._model = model
        self._armature_obj = None
        self._instances = dict()

        bpy.context.scene.collection.children.link(self._root_collection)

    def _create_collection(self, name: str, key: int = None) -> Collection:
        if key != None:
            name = f'{self._root_collection.name}::{name}'
        collection = bpy.data.collections.new(name) # TODO: enforce unique model names
        if key != None:
            self._region_collections[key] = collection
            self._root_collection.children.link(collection)
        return collection

    def _get_bone_transforms(self) -> List[Matrix]:
        result = []
        for bone in self._model.bones:
            lineage = self._model.get_bone_lineage(bone)
            transforms = [_convert_transform_units(x.transform, True) for x in lineage]
            result.append(reduce(operator.matmul, transforms))
        return result

    def create_bones(self):
        context, collection, scene, model = self._context, self._root_collection, self._scene, self._model

        # OPTIONS.BONE_SCALE not relevant to blender since you cant set bone width?
        TAIL_VECTOR = (0.03 * UNIT_SCALE, 0.0, 0.0)

        set_active_collection(self._root_collection)
        bone_transforms = self._get_bone_transforms()

        bpy.ops.object.add(type = 'ARMATURE', enter_editmode = True)
        self._armature_obj = context.object
        armature_data = cast(Armature, context.object.data)
        armature_data.name = f'{OPTIONS.model_name(model)} armature'

        editbones = [armature_data.edit_bones.new(OPTIONS.bone_name(b)) for b in model.bones]
        for i, b in enumerate(model.bones):
            editbone = editbones[i]
            editbone.tail = TAIL_VECTOR
            
            children = model.get_bone_children(b)
            if children:
                size = max((Vector(b.transform[3]).to_3d().length for b in children))
                editbone.tail = (size * UNIT_SCALE, 0, 0)

            editbone.transform(bone_transforms[i])

            if b.parent_index >= 0:
                editbone.parent = editbones[b.parent_index]

        bpy.ops.object.mode_set(mode = 'OBJECT')

    def create_markers(self):
        context, model = self._context, self._model

        MODE = 'EMPTY_SPHERE' # TODO
        MARKER_SIZE = 0.01 * UNIT_SCALE * OPTIONS.MARKER_SCALE

        set_active_collection(self._root_collection)
        bone_transforms = self._get_bone_transforms()

        for marker in model.markers:
            for i, instance in enumerate(marker.instances):
                # attempt to creat the marker within the appropriate collection based on region/permutation
                # note that the collection acts like a 'parent' so if the marker gets parented to a bone it gets removed from the collection
                if instance.region_index >= 0 and instance.region_index < 255:
                    set_active_collection(self._region_collections[instance.region_index])
                else:
                    set_active_collection(self._root_collection)

                if MODE == 'EMPTY_SPHERE':
                    # despite the parameter being named 'radius' it comes out the same size as a uvsphere's diameter
                    bpy.ops.object.empty_add(type = 'SPHERE', radius = MARKER_SIZE)
                    marker_obj = context.object
                    marker_obj.name = OPTIONS.marker_name(marker, i)
                # else: TODO

                world_transform = Matrix.Translation([v * UNIT_SCALE for v in instance.position]) @ Quaternion(instance.rotation).to_matrix().to_4x4()

                if instance.bone_index >= 0:
                    world_transform = bone_transforms[instance.bone_index] @ world_transform
                    if OPTIONS.IMPORT_BONES and self._armature_obj:
                        marker_obj.parent = self._armature_obj
                        marker_obj.parent_type = 'BONE'
                        marker_obj.parent_bone = OPTIONS.bone_name(model.bones[instance.bone_index])

                marker_obj.hide_render = True
                marker_obj.matrix_world = world_transform

    def create_meshes(self):
        for i, r in enumerate(self._model.regions):
            region_col = self._create_collection(OPTIONS.region_name(r), i)
            for p in r.permutations:
                self._build_mesh(region_col, r, p)

    def _build_mesh(self, collection: Collection, region: ModelRegion, permutation: ModelPermutation):
        context, scene, model = self._context, self._scene, self._model

        SPLIT_MODE = False # TODO

        WORLD_TRANSFORM = _convert_transform_units(permutation.transform)

        for mesh_index in range(permutation.mesh_index, permutation.mesh_index + permutation.mesh_count):
            MESH_NAME = OPTIONS.permutation_name(region, permutation, mesh_index)
            INSTANCE_KEY = (mesh_index, -1) # TODO: second element reserved for submesh index if mesh splitting enabled

            if INSTANCE_KEY in self._instances.keys():
                source = self._instances.get(INSTANCE_KEY)
                copy = cast(Object, source.copy()) # note: use source.data.copy() for a deep copy
                copy.name = MESH_NAME
                copy.matrix_world = WORLD_TRANSFORM
                collection.objects.link(copy)
                continue

            mesh = model.meshes[mesh_index]
            index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]
            vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

            # note blender doesnt like if we provide too many dimensions
            positions = list(Vector(v).to_3d() for v in vertex_buffer.position_channels[0])
            normals = list(Vector(v).to_3d() for v in vertex_buffer.normal_channels[0])
            faces = list(index_buffer.get_triangles(mesh))

            mesh_data = bpy.data.meshes.new(MESH_NAME)
            mesh_data.from_pydata(positions, [], faces)

            SELF_TRANSFORM = Matrix(mesh.vertex_transform).transposed()
            mesh_data.transform(SELF_TRANSFORM)

            for p in mesh_data.polygons:
                p.use_smooth = True

            if OPTIONS.IMPORT_NORMALS:
                mesh_data.normals_split_custom_set_from_vertices(normals)
                mesh_data.use_auto_smooth = True # this is required in order for custom normals to take effect

            mesh_obj = bpy.data.objects.new(mesh_data.name, mesh_data)
            mesh_obj.matrix_world = WORLD_TRANSFORM
            collection.objects.link(mesh_obj)

            if OPTIONS.IMPORT_BONES and OPTIONS.IMPORT_SKIN and self._armature_obj and (vertex_buffer.blendindex_channels or mesh.bone_index >= 0):
                modifier = cast(bpy.types.ArmatureModifier, mesh_obj.modifiers.new(f'{mesh_data.name}::armature', 'ARMATURE'))
                modifier.object = self._armature_obj

                if mesh.bone_index >= 0:
                    # only need one vertex group
                    bone = model.bones[mesh.bone_index]
                    group = mesh_obj.vertex_groups.new(name=bone.name)
                    group.add(range(len(positions)), 1.0, 'ADD') # set every vertex to 1.0 in one go
                else:
                    blend_indicies = vertex_buffer.blendindex_channels[0]
                    blend_weights = vertex_buffer.blendweight_channels[0]
                    # create a vertex group for each bone so the bone indices are 1:1 with the vertex groups
                    for bone in model.bones:
                        mesh_obj.vertex_groups.new(name=bone.name)
                    for i in range(len(positions)):
                        for bi, bw in zip(blend_indicies[i], blend_weights[i]):
                            if bw > 0:
                                mesh_obj.vertex_groups[bi].add([i], bw, 'ADD')

            self._instances[INSTANCE_KEY] = mesh_obj

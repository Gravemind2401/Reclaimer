import bpy
import bmesh
import itertools
import operator
from typing import cast
from typing import Dict, Tuple
from mathutils import Vector, Matrix
from bpy.types import Context, Collection, Armature, EditBone, Object
from functools import reduce

from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *

BL_UNITS = 1000.0 # 1 blender unit = 1000mm

def create_model(context: Context, scene: Scene, model: Model):
    builder = ModelBuilder(context, scene, model)
    builder.create_bones()
    builder.create_meshes()

class ModelBuilder:
    _context: Context
    _collection: Collection
    _scene: Scene
    _model: Model
    _armature_data: Armature
    _instances: Dict[Tuple[int, int], Object]

    def __init__(self, context: Context, scene: Scene, model: Model):
        self._context = context
        self._collection = bpy.data.collections.new(model.name)
        self._scene = scene
        self._model = model
        self._armature_data = None
        self._instances = dict()
        bpy.context.scene.collection.children.link(self._collection)

    def create_bones(self):
        context, collection, scene, model = self._context, self._collection, self._scene, self._model
        print('creating armature')

        UNIT_SCALE = scene.unit_scale / BL_UNITS
        PREFIX = '' # TODO
        TAIL_VECTOR = (0.03 * UNIT_SCALE, 0.0, 0.0)

        bpy.ops.object.add(type = 'ARMATURE', enter_editmode = True)
        armature_obj = context.object
        armature_data = self._armature_data = cast(Armature, context.object.data)
        armature_data.name = f'{model.name} armature'

        def makemat(mat) -> Matrix:
            m = Matrix(mat).transposed()
            translation, rotation, scale = m.decompose()
            return Matrix.Translation(translation * UNIT_SCALE) @ rotation.to_matrix().to_4x4()
        
        bone_transforms = []
        for b in model.bones:
            lineage = model.get_bone_lineage(b)
            lst = [makemat(x.transform) for x in lineage]
            bone_transforms.append(reduce(operator.matmul, lst))

        editbones = [armature_data.edit_bones.new(PREFIX + b.name) for b in model.bones]
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

        # bpy.ops.object.add() will automatically add it to the active collection
        # so remove it and add it to the custom collection instead
        context.collection.objects.unlink(armature_obj)
        collection.objects.link(armature_obj)

    def create_markers(self):
        context, model = self._context, self._model
        print('creating markers')
        
        PREFIX = '' # TODO
        MODE = 'MESH' # TODO

        for m in model.markers:
            NAME = PREFIX + m.name
            for inst in m.instances:
                pass # TODO

    def create_meshes(self):
        collection, model = self._collection, self._model

        for r in model.regions:
            region_col = bpy.data.collections.new(r.name)
            collection.children.link(region_col)
            for p in r.permutations:
                self._build_mesh(region_col, r, p)

    def _build_mesh(self, collection: Collection, region: ModelRegion, permutation: ModelPermutation):
        context, scene, model = self._context, self._scene, self._model
        
        UNIT_SCALE = scene.unit_scale / BL_UNITS
        SPLIT_MODE = False # TODO

        for mesh_index in range(permutation.mesh_index, permutation.mesh_index + permutation.mesh_count):
            MESH_NAME = f'{region.name}:{permutation.name}'
            INSTANCE_KEY = (mesh_index, -1) # TODO: second element reserved for submesh index if mesh splitting enabled

            if INSTANCE_KEY in self._instances.keys():
                source = self._instances.get(INSTANCE_KEY)
                copy = cast(Object, source.copy()) # note: use source.data.copy() for a deep copy
                copy.name = MESH_NAME
                copy.matrix_world = permutation.transform
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

            mesh_transform = Matrix.Scale(UNIT_SCALE, 4) @ Matrix(mesh.vertex_transform).transposed()
            mesh_data.transform(mesh_transform)

            for p in mesh_data.polygons:
                p.use_smooth = True

            mesh_data.normals_split_custom_set_from_vertices(normals)
            mesh_data.use_auto_smooth = True # this is required in order for custom normals to take effect

            mesh_obj = bpy.data.objects.new(mesh_data.name, mesh_data)
            mesh_obj.matrix_world = permutation.transform
            collection.objects.link(mesh_obj)

            self._instances[INSTANCE_KEY] = mesh_obj

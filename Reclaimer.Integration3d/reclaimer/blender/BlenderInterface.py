import bpy
import itertools
import operator
from typing import cast
from typing import Dict, Tuple, List
from mathutils import Vector, Matrix, Quaternion
from bpy.types import Context, Collection, Armature, EditBone, Object
from functools import reduce

from .CustomShaderNodes import *
from .MaterialBuilder import *
from .Utils import *

from ..src.ImportOptions import *
from ..src.SceneFilter import *
from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *
from ..src.Progress import *
from ..src.ViewportInterface import *

__all__ = [
    'BlenderInterface'
]


BL_UNITS: float = 1000.0 # 1 blender unit = 1000mm

MeshContext = Tuple[Scene, 'BlenderModelState', MeshParams, bpy.types.Mesh, Object]


class BlenderModelState(ModelState):
    parent_collection: Collection
    root_object: Object
    region_objects: Dict[int, Object]
    armature_obj: Object

    def __init__(self, model: Model, filter: ModelFilter, display_name: str, collection: Collection):
        super().__init__(model, filter, display_name)
        self.parent_collection = collection
        self.root_object = self.create_group_object(display_name)
        self.region_objects = dict()
        self.armature_obj = None

    def link_object(self, object: Object, parent: Object):
        self.parent_collection.objects.link(object)
        object.parent = parent

    def create_group_object(self, name: str) -> Object:
        group = bpy.data.objects.new(name, None)
        group.hide_render = True
        self.link_object(group, None)
        return group


class BlenderInterface(ViewportInterface[bpy.types.Material, bpy.types.Collection, Matrix, BlenderModelState, bpy.types.Object]):
    unit_scale: float = 1.0
    scene: Scene = None
    options: ImportOptions = None
    root_collection: bpy.types.Collection = None
    material_builder: MaterialBuilder = None
    materials: List[bpy.types.Material] = None
    unique_meshes: Dict[MeshKey, Object] = None

    def _apply_custom_properties(self, target, source: ICustomProperties):
        if self.options.IMPORT_CUSTOM_PROPS:
            for k, v in source.custom_properties.items():
                target[k] = v

    def init_scene(self, scene: Scene, options: ImportOptions) -> None:
        self.unit_scale = scene.unit_scale / BL_UNITS
        self.scene = scene
        self.options = options
        self.unique_meshes = dict()

    def pre_import(self, root_collection: bpy.types.Collection):
        self.root_collection = root_collection

        # temporarily exclude from view layer so it doesnt redraw constantly - this improves speed a lot
        # tried setting hide_viewport instead but that causes markers to break for some reason
        set_collection_exclude(bpy.context.view_layer, self.root_collection, True)

    def post_import(self):
        set_collection_exclude(bpy.context.view_layer, self.root_collection, False)

    def init_materials(self) -> None:
        init_custom_node_groups()
        self.material_builder = MaterialBuilder(self.scene, self.options)

    def create_material(self, material: Material) -> bpy.types.Material:
        return self.material_builder.create_material(material)

    def set_materials(self, materials: List[bpy.types.Material]) -> None:
        self.materials = materials

    def create_collection(self, display_name: str, parent: Collection) -> Collection:
        if not parent:
            parent = bpy.context.scene.collection
        collection = bpy.data.collections.new(display_name)
        parent.children.link(collection)
        return collection

    def identity_transform(self) -> Matrix:
        return Matrix.Identity(4)

    def invert_transform(self, transform: Matrix) -> Matrix:
        return transform.inverted()

    def multiply_transform(self, a: Matrix, b: Matrix) -> Matrix:
        return a @ b

    def create_transform(self, transform: Matrix4x4, bone_mode: bool = False) -> Matrix:
        if not bone_mode:
            return Matrix.Scale(self.unit_scale, 4) @ Matrix(transform).transposed()

        # for bones we want to keep the scale component at 1x, but still need to convert the translation component
        m = Matrix(transform).transposed()
        translation, rotation, scale = m.decompose()
        return Matrix.Translation(translation * self.unit_scale) @ rotation.to_matrix().to_4x4()

    def init_model(self, model: Model, filter: ModelFilter, collection: Collection, display_name: str) -> BlenderModelState:
        state = BlenderModelState(model, filter, display_name, collection)
        self._apply_custom_properties(state.root_object, model)
        return state

    def apply_transform(self, model_state: BlenderModelState, world_transform: Matrix) -> None:
        model_state.root_object.matrix_world = world_transform
        for c in model_state.root_object.children:
            c.matrix_parent_inverse = Matrix.Identity(4)

    def _get_bone_transforms(self, model: Model) -> List[Matrix]:
        result = []
        for bone in model.bones:
            lineage = model.get_bone_lineage(bone)
            transforms = [self.create_transform(x.transform, True) for x in lineage]
            result.append(reduce(operator.matmul, transforms))
        return result

    def create_bones(self, model_state: BlenderModelState) -> None:
        model, group_obj = model_state.model, model_state.root_object

        # options.BONE_SCALE not relevant to blender since you cant set bone width?
        TAIL_VECTOR = (0.03 * self.unit_scale, 0.0, 0.0)

        bone_transforms = self._get_bone_transforms(model)

        # armatures only work while they are included in the view layer
        # so we need to temporaily include the parent collection until the armature is done
        set_collection_exclude(bpy.context.view_layer, model_state.parent_collection, False)

        armature_data = bpy.data.armatures.new(f'{model_state.display_name} armature root')
        armature_obj = model_state.armature_obj = bpy.data.objects.new(f'{model_state.display_name} armature', armature_data)
        model_state.link_object(armature_obj, group_obj)

        # edit mode is mandatory for edit_bone management, and the armature object needs to be selected to enable edit mode
        bpy.ops.object.select_all(action = 'DESELECT')
        armature_obj.select_set(True)
        bpy.context.view_layer.objects.active = armature_obj
        bpy.ops.object.mode_set(mode = 'EDIT')

        editbones = list(armature_data.edit_bones.new(self.options.bone_name(b)) for b in model.bones)
        for i, b in enumerate(model.bones):
            editbone = editbones[i]
            editbone.tail = TAIL_VECTOR

            children = model.get_bone_children(b)
            if children:
                size = max((Vector(b.transform[3]).to_3d().length for b in children))
                editbone.tail = (size * self.unit_scale, 0, 0)

            editbone.transform(bone_transforms[i])

            if b.parent_index >= 0:
                editbone.parent = editbones[b.parent_index]

            self._apply_custom_properties(editbone, b)

        bpy.ops.object.mode_set(mode = 'OBJECT')
        set_collection_exclude(bpy.context.view_layer, model_state.parent_collection, True)

    def create_markers(self, model_state: BlenderModelState) -> None:
        options, model = self.options, model_state.model

        MODE = 'EMPTY_SPHERE' # TODO
        MARKER_SIZE = 0.01 * self.unit_scale * options.MARKER_SCALE

        bone_transforms = self._get_bone_transforms(model)

        for marker in model.markers:
            for i, instance in enumerate(marker.instances):
                # attempt to create the marker within the appropriate collection based on region/permutation
                # note that in blender the collection acts like a 'parent' so if the marker gets parented to a bone it gets removed from the collection
                group_obj = model_state.region_objects.get(instance.region_index, model_state.root_object)

                if MODE == 'EMPTY_SPHERE':
                    marker_obj = bpy.data.objects.new(options.marker_name(marker, i), None)
                    marker_obj.empty_display_type = 'SPHERE'
                    marker_obj.empty_display_size = MARKER_SIZE
                    model_state.link_object(marker_obj, group_obj)
                # else: TODO

                world_transform = Matrix.Translation([v * self.unit_scale for v in instance.position]) @ Quaternion(instance.rotation).to_matrix().to_4x4()

                if instance.bone_index >= 0 and model.bones:
                    world_transform = bone_transforms[instance.bone_index] @ world_transform
                    if options.IMPORT_BONES:
                        marker_obj.parent = model_state.armature_obj
                        marker_obj.parent_type = 'BONE'
                        marker_obj.parent_bone = options.bone_name(model.bones[instance.bone_index])

                marker_obj.hide_render = True
                marker_obj.matrix_world = world_transform

                # instance properties will replace group properties if they have the same name
                self._apply_custom_properties(marker_obj, marker)
                self._apply_custom_properties(marker_obj, instance)

    def create_region(self, model_state: BlenderModelState, region: ModelRegion, display_name: str) -> Object:
        region_obj = model_state.create_group_object(display_name)
        region_obj.parent = model_state.root_object
        model_state.region_objects[model_state.model.regions.index(region)] = region_obj
        self._apply_custom_properties(region_obj, region)
        return region_obj

    def build_mesh(self, model_state: BlenderModelState, permutation: ModelPermutation, region_group: Object, world_transform: Matrix, mesh_params: MeshParams) -> None:
        vertex_buffer, mesh_key, display_name = mesh_params.vertex_buffer, mesh_params.mesh_key, mesh_params.display_name

        existing_mesh = self.unique_meshes.get(mesh_key, None)
        if existing_mesh:
            copy = cast(Object, existing_mesh.copy()) # note: use source.data.copy() for a deep copy
            copy.name = display_name
            model_state.link_object(copy, region_group)
            copy.matrix_world = world_transform
            copy.matrix_parent_inverse = Matrix.Identity(4)
            self._apply_custom_properties(copy, permutation)
            return

        # note blender doesnt like if we provide too many dimensions
        positions = list(Vector(v).to_3d() for v in vertex_buffer.position_channels[0])
        faces = list(mesh_params.chain_triangles())

        mesh_data = bpy.data.meshes.new(display_name)
        mesh_data.from_pydata(positions, [], faces)

        DECOMPRESSION_TRANSFORM = Matrix(mesh_params.vertex_transform).transposed()
        mesh_data.transform(DECOMPRESSION_TRANSFORM)

        for p in mesh_data.polygons:
            p.use_smooth = True

        mesh_obj = bpy.data.objects.new(mesh_data.name, mesh_data)
        mesh_obj.matrix_world = world_transform
        model_state.link_object(mesh_obj, region_group)
        self.unique_meshes[mesh_key] = mesh_obj

        self._apply_custom_properties(mesh_obj, permutation)

        mc: MeshContext = (self.scene, model_state, mesh_params, mesh_data, mesh_obj)
        self._build_normals(mc)
        self._build_uvw(mc, faces)
        self._build_matindex(mc)
        self._build_skin(mc)
        self._build_colors(mc, faces)

    def _build_normals(self, mc: MeshContext):
        scene, model_state, mesh_params, mesh_data, mesh_obj = mc
        vertex_buffer = mesh_params.vertex_buffer

        if not (self.options.IMPORT_NORMALS and vertex_buffer.normal_channels):
            return

        normals = list(Vector(v).to_3d() for v in vertex_buffer.normal_channels[0])
        mesh_data.normals_split_custom_set_from_vertices(normals)

        # prior to 4.1, this is required in order for custom normals to take effect
        # it was removed in 4.1 and custom normals should work normally
        if bpy.app.version < (4, 1):
            mesh_data.use_auto_smooth = True

    def _build_uvw(self, mc: MeshContext, faces: List[Triangle]):
        scene, model_state, mesh_params, mesh_data, mesh_obj = mc
        vertex_buffer = mesh_params.vertex_buffer

        if not (self.options.IMPORT_UVW and vertex_buffer.texcoord_channels):
            return

        DECOMPRESSION_TRANSFORM = Matrix(mesh_params.texture_transform).transposed()

        for texcoord_buffer in vertex_buffer.texcoord_channels:
            # note blender wants 3 uvs per triangle rather than one per vertex
            # so we iterate the triangle indices rather than directly iterating the buffer
            uv_layer = mesh_data.uv_layers.new()
            for i, ti in enumerate(itertools.chain(*faces)):
                v = texcoord_buffer[ti]
                vec = DECOMPRESSION_TRANSFORM @ Vector((v[0], v[1], 0))
                uv_layer.data[i].uv = Vector((vec[0], 1 - vec[1]))

    def _build_matindex(self, mc: MeshContext):
        scene, model_state, mesh_params, mesh_data, mesh_obj = mc

        if not self.options.IMPORT_MATERIALS:
            return

        # only append materials to the mesh that it actually uses, rather than appening all scene materials
        # this means we need to build a lookup of global mat index -> local mat index
        mat_lookup = dict()
        for loc, glob in enumerate(set(mi for mi, _ in mesh_params.triangle_sets if mi >= 0)):
            mat_lookup[glob] = loc

        if not mat_lookup:
            return # no materials on this mesh

        # append relevant material(s) to mesh
        for i in mat_lookup.keys():
            mesh_data.materials.append(self.materials[i])

        face_start = 0
        for mi, triangles in mesh_params.triangle_sets:
            face_end = face_start + len(triangles)
            if mi >= 0:
                for i in range(face_start, face_end):
                    mesh_data.polygons[i].material_index = mat_lookup[mi]
            face_start = face_end

    def _build_skin(self, mc: MeshContext):
        scene, model_state, mesh_params, mesh_data, mesh_obj = mc
        vertex_buffer, bone_index = mesh_params.vertex_buffer, mesh_params.bone_index

        if not (
            self.options.IMPORT_BONES
            and self.options.IMPORT_SKIN
            and model_state.model.bones
            and (vertex_buffer.blendindex_channels or bone_index >= 0)
        ):
            return

        vertex_count = len(vertex_buffer.position_channels[0])

        modifier = cast(bpy.types.ArmatureModifier, mesh_obj.modifiers.new(f'{mesh_data.name}::armature', 'ARMATURE'))
        modifier.object = model_state.armature_obj

        if bone_index >= 0:
            # only need one vertex group
            bone = model_state.model.bones[bone_index]
            group = mesh_obj.vertex_groups.new(name=bone.name)
            group.add(range(vertex_count), 1.0, 'ADD') # set every vertex to 1.0 in one go
        else:
            # create a vertex group for each bone so the bone indices are 1:1 with the vertex groups
            for bone in model_state.model.bones:
                mesh_obj.vertex_groups.new(name=bone.name)
            for vi, blend_indicies, blend_weights in vertex_buffer.enumerate_blendpairs():
                for bi, bw in zip(blend_indicies, blend_weights):
                    mesh_obj.vertex_groups[bi].add([vi], bw, 'ADD')

    def _build_colors(self, mc: MeshContext, faces: List[Triangle]):
        scene, model_state, mesh_params, mesh_data, mesh_obj = mc
        vertex_buffer = mesh_params.vertex_buffer

        if not (self.options.IMPORT_COLORS and vertex_buffer.color_channels):
            return

        for color_buffer in vertex_buffer.color_channels:
            # note vertex_colors uses the same triangle loop as uv coords
            # so we iterate the triangle indices rather than directly iterating the buffer
            color_layer = mesh_data.vertex_colors.new()
            for i, ti in enumerate(itertools.chain(*faces)):
                c = color_buffer[ti]
                color_layer.data[i].color = c
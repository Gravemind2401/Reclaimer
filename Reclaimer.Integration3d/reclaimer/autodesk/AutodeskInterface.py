import operator
from typing import cast
from typing import Dict, Tuple, List
from functools import reduce

import pymxs
from pymxs import runtime as rt

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
    'AutodeskInterface'
]


MX_UNITS = 100.0 # 1 max unit = 100mm?

MeshContext = Tuple[Scene, 'AutodeskModelState', Mesh, rt.Editable_Mesh]
Layer = rt.MixinInterface


class AutodeskModelState(ModelState):
    root_layer: Layer
    region_layers: Dict[int, Layer]
    maxbones = List[rt.BoneGeometry]

    def __init__(self, model: Model, filter: ModelFilter, display_name: str, layer: Layer):
        super().__init__(model, filter, display_name)
        self.root_layer = self.create_layer(display_name)
        self.root_layer.setParent(layer)
        self.region_layers = dict()

    def create_layer(self, name: str) -> Layer:
        layer = rt.LayerManager.newLayerFromName(name)
        return layer


class AutodeskInterface(ViewportInterface[rt.Material, Layer, rt.Matrix3, AutodeskModelState, Layer]):
    unit_scale: float = 1.0
    scene: Scene = None
    options: ImportOptions = None
    materials: List[rt.Material] = None
    unique_meshes: Dict[MeshKey, rt.Mesh] = None

    def init_scene(self, scene: Scene, options: ImportOptions) -> None:
        self.unit_scale = scene.unit_scale / MX_UNITS
        self.scene = scene
        self.options = options
        self.unique_meshes = dict()

    def init_materials(self) -> None:
        pass

    def create_material(self, material: Material) -> rt.Material:
        return None

    def set_materials(self, materials: List[rt.Material]) -> None:
        self.materials = materials

    def identity_transform(self) -> rt.Matrix3:
        return rt.Matrix3(1)

    def invert_transform(self, transform: rt.Matrix3) -> rt.Matrix3:
        return rt.inverse(transform)

    def multiply_transform(self, a: rt.Matrix3, b: rt.Matrix3) -> rt.Matrix3:
        return b * a

    def create_transform(self, transform: Matrix4x4, bone_mode: bool = False) -> rt.Matrix3:
        if not bone_mode:
            return toMatrix3(transform) * rt.scaleMatrix(rt.Point3(self.unit_scale, self.unit_scale, self.unit_scale))

        # for bones we want to keep the scale component at 1x, but still need to convert the translation component
        m = toMatrix3(transform)
        return rt.preRotate(rt.transMatrix(m.translationPart * self.unit_scale), m.rotationPart)

    def init_model(self, model: Model, filter: ModelFilter, collection: rt.MixinInterface, display_name: str) -> AutodeskModelState:
        rt.gc()
        state = AutodeskModelState(model, filter, display_name, collection)
        return state

    def apply_transform(self, model_state: AutodeskModelState, world_transform: rt.Matrix3) -> None:
        pass

    def _get_bone_transforms(self, model: Model) -> List[rt.Matrix3]:
        result = []
        for bone in model.bones:
            lineage = model.get_bone_lineage(bone)
            transforms = [self.create_transform(x.transform, True) for x in reversed(lineage)]
            result.append(reduce(operator.mul, transforms))
        return result

    def create_bones(self, model_state: AutodeskModelState) -> None:
        model = model_state.model

        BONE_SIZE = 0.03 * self.unit_scale * self.options.BONE_SCALE
        TAIL_VECTOR = rt.Point3(BONE_SIZE, 0.0, 0.0)

        bone_layer = model_state.create_layer(f'{model_state.display_name}::__bones__')
        bone_layer.setParent(model_state.root_layer)
        model_state.region_layers[-1] = bone_layer
        bone_transforms = self._get_bone_transforms(model)

        maxbones = model_state.maxbones = []
        for i, b in enumerate(model.bones):
            maxbone = rt.BoneSys.createBone(rt.Point3(0, 0, 0), TAIL_VECTOR, rt.Point3(0, 0, 1))
            maxbone.setBoneEnable(False, 0)
            maxbone.name = self.options.bone_name(b)
            maxbone.height = maxbone.width = maxbone.length = BONE_SIZE
            maxbones.append(maxbone)
            bone_layer.addnode(maxbone)

            children = model.get_bone_children(b)
            if children:
                size = max(rt.length(toPoint3(b.transform[3])) for b in children)
                maxbone.length = size * self.unit_scale

            maxbone.taper = 70 if children else 50
            maxbone.transform = bone_transforms[i]

            if b.parent_index >= 0:
                maxbone.parent = maxbones[b.parent_index]

    def create_markers(self, model_state: AutodeskModelState) -> None:
        options, model = self.options, model_state.model

        MARKER_SIZE = 0.01 * self.unit_scale * options.MARKER_SCALE

        marker_layer = None
        bone_transforms = self._get_bone_transforms(model)

        for marker in model.markers:
            for i, instance in enumerate(marker.instances):
                marker_obj = rt.Sphere(radius = MARKER_SIZE)
                marker_obj.name = options.marker_name(marker, i)

                # put the marker in the appropriate layer based on region/permutation
                if instance.region_index >= 0 and instance.region_index < 255:
                    model_state.region_layers[instance.region_index].addnode(marker_obj)
                else:
                    if not marker_layer:
                        marker_layer = model_state.create_layer(f'{model_state.display_name}::__markers__')
                        marker_layer.setParent(model_state.root_layer)
                        model_state.region_layers[-2] = marker_layer
                    marker_layer.addnode(marker_obj)

                world_transform = rt.preRotate(rt.transMatrix(toPoint3(instance.position) * self.unit_scale), toQuat(instance.rotation))

                if instance.bone_index >= 0 and model.bones:
                    world_transform *= bone_transforms[instance.bone_index]
                    if options.IMPORT_BONES:
                        marker_obj.parent = model_state.maxbones[instance.bone_index]

                marker_obj.renderable = False
                marker_obj.transform = world_transform

    def create_region(self, model_state: AutodeskModelState, region: ModelRegion, display_name: str) -> rt.MixinInterface:
        region_layer = model_state.create_layer(display_name)
        region_layer.setParent(model_state.root_layer)
        model_state.region_layers[model_state.model.regions.index(region)] = region_layer
        return region_layer

    def build_mesh(self, model_state: AutodeskModelState, permutation: ModelPermutation, region_group: Layer, world_transform: rt.Matrix3, mesh: Mesh, mesh_key: MeshKey, display_name: str) -> None:
        scene = self.scene

        DECOMPRESSION_TRANSFORM = toMatrix3(mesh.vertex_transform)

        if mesh_key in self.unique_meshes.keys():
            source = self.unique_meshes.get(mesh_key)
            # methods with byref params return a tuple of (return_value, byref1, byref2, ...)
            _, newNodes = rt.MaxOps.cloneNodes(source, cloneType = rt.Name('instance'), newNodes = pymxs.byref(None))
            copy = cast(rt.Mesh, newNodes[0])
            copy.name = display_name
            copy.transform = DECOMPRESSION_TRANSFORM * world_transform
            region_group.addnode(copy)
            return

        index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]
        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

        # note 3dsMax uses 1-based indices for triangles, vertices etc

        positions = list(toPoint3(v) for v in vertex_buffer.position_channels[0])
        faces = list(toPoint3(t) + 1 for t in index_buffer.get_triangles(mesh))

        mesh_obj = cast(rt.Editable_Mesh, rt.Mesh(vertices=positions, faces=faces))
        mesh_obj.name = display_name
        mesh_obj.transform = DECOMPRESSION_TRANSFORM
        region_group.addnode(mesh_obj)
        self.unique_meshes[mesh_key] = mesh_obj

        mc: MeshContext = (scene, model_state, mesh, mesh_obj)
        self._build_normals(mc)
        self._build_uvw(mc)
        self._build_matindex(mc)
        self._build_skin(mc)
        self._build_colors(mc)

        # need to decompress BEFORE applying normals, then apply the instance transform AFTER applying normals
        mesh_obj.transform = DECOMPRESSION_TRANSFORM * world_transform

    def _build_normals(self, mc: MeshContext):
        scene, model_state, mesh, mesh_obj = mc
        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

        if not (self.options.IMPORT_NORMALS and vertex_buffer.normal_channels):
            return

        normals = list(toPoint3(v) for v in vertex_buffer.normal_channels[0])
        for i, normal in enumerate(normals):
            # note: prior to 2015, this was only temporary
            rt.setNormal(mesh_obj, i + 1, normal)

    def _build_uvw(self, mc: MeshContext):
        scene, model_state, mesh, mesh_obj = mc
        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

        if not (self.options.IMPORT_UVW and vertex_buffer.texcoord_channels):
            return

        DECOMPRESSION_TRANSFORM = toMatrix3(mesh.texture_transform)

        # unlike other areas, uvw maps/channels use 0-based indexing
        # however channel 0 is always reserved for vertex color
        rt.Meshop.setNumMaps(mesh_obj, len(vertex_buffer.texcoord_channels) + 1)

        for i, texcoord_buffer in enumerate(vertex_buffer.texcoord_channels):
            rt.Meshop.defaultMapFaces(mesh_obj, i + 1) # sets vert/face count to same as mesh, copies triangle indices from mesh
            for vi, v in enumerate(texcoord_buffer):
                vec = rt.Point3(v[0], v[1], 0) * DECOMPRESSION_TRANSFORM
                rt.Meshop.setMapVert(mesh_obj, i + 1, vi + 1, rt.Point3(vec[0], 1 - vec[1], 0))

    def _build_matindex(self, mc: MeshContext):
        scene, model_state, mesh, mesh_obj = mc
        index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]

        if not self.options.IMPORT_MATERIALS:
            return

        material_ids = []
        for s in mesh.segments:
            # TODO: more efficient triangle count
            mi = max(1, s.material_index + 1) # default to 1 for meshes with no material
            material_ids.extend(mi for _ in range(index_buffer.count_triangles(s)))

        rt.setMesh(mesh_obj, materialIds=material_ids)

    def _build_skin(self, mc: MeshContext):
        scene, model_state, mesh, mesh_obj = mc
        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

        if not (
            self.options.IMPORT_BONES
            and self.options.IMPORT_SKIN
            and model_state.model.bones
            and (vertex_buffer.blendindex_channels or mesh.bone_index >= 0)
        ):
            return

        vertex_count = len(vertex_buffer.position_channels[0])

        modifier = rt.Skin()
        rt.addModifier(mesh_obj, modifier)

        # note replaceVertexWeights() can take either bone indices or bone references
        if mesh.bone_index >= 0:
            modifier.rigid_vertices = True
            bi, bw = [model_state.maxbones[mesh.bone_index]], [1.0]
            rt.SkinOps.addBone(modifier, bi[0], 0)
            rt.redrawViews()
            for vi in range(vertex_count): # set every vertex to 1.0
                rt.SkinOps.replaceVertexWeights(modifier, vi + 1, bi, bw)
        else:
            # add every bone so the bone indices are 1:1 with the skin modifier
            for b in model_state.maxbones:
                rt.SkinOps.addBone(modifier, b, 0)
            # unfortunately it seems a redraw is required for the added bones to take effect
            # otherwise trying to set weights gives the error "Runtime error: Exceeded the vertex countSkin:Skin"
            rt.redrawViews()
            bi, bw = [], []
            for vi, blend_indicies, blend_weights in vertex_buffer.enumerate_blendpairs():
                bi.clear()
                bw.clear()
                for i, w in enumerate(blend_weights):
                    bi.append(model_state.maxbones[blend_indicies[i]])
                    bw.append(w)
                rt.SkinOps.replaceVertexWeights(modifier, vi + 1, bi, bw)

        rt.SkinOps.removeUnusedBones(modifier)

    def _build_colors(self, mc: MeshContext):
        pass # TODO
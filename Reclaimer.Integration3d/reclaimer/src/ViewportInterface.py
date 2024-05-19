import itertools
from typing import TypeVar, Generic, Tuple, List, Iterator

from .ImportOptions import *
from .SceneFilter import *
from .Scene import *
from .Model import *
from .Material import *
from .Types import *
from .VertexBuffer import *
from .IndexBuffer import *
from .Progress import *

__all__ = [
    'MeshKey',
    'ModelState',
    'MeshParams',
    'ViewportInterface'
]

MeshKey = Tuple[int, int, int] # model index, mesh index, segment index


class ModelState():
    model: Model
    filter: ModelFilter
    display_name: str

    def __init__(self, model: Model, filter: ModelFilter, display_name: str) -> None:
        self.model = model
        self.filter = filter
        self.display_name = display_name


TMaterial = TypeVar('TMaterial')
TCollection = TypeVar('TCollection')
TMatrix = TypeVar('TMatrix')
TModelState = TypeVar('TModelState', bound=ModelState)
TRegionGroup = TypeVar('TRegionGroup')


class MeshParams:
    source_mesh: Mesh
    source_segment: MeshSegment
    bone_index: int
    vertex_transform: Matrix4x4
    texture_transform: Matrix4x4
    vertex_buffer: VertexBuffer
    index_buffer: IndexBuffer
    mesh_key: MeshKey
    display_name: str
    triangle_sets: List[Tuple[int, List[Triangle]]] # list of (material_index, triangles)

    def __init__(self, scene: Scene, mesh: Mesh, segment: MeshSegment, mesh_key: MeshKey, display_name: str) -> None:
        self.source_mesh = mesh
        self.source_segment = segment
        self.bone_index = mesh.bone_index
        self.vertex_transform = mesh.vertex_transform
        self.texture_transform = mesh.texture_transform
        self.mesh_key = mesh_key
        self.display_name = display_name

        vertex_buffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]
        index_buffer = scene.index_buffer_pool[mesh.index_buffer_index]

        if segment is None:
            # use the entire mesh
            self.vertex_buffer = vertex_buffer
            self.index_buffer = index_buffer
            self.triangle_sets = [(s.material_index, list(self.index_buffer.get_triangles(s))) for s in mesh.segments]
        else:
            # only use a specific mesh segment
            # note that this can result in unused verts
            # for example, a segment with a single triangle of 0-8-9 would need all 10 vertices in order to keep the indices in alignment
            # detecting+removing unused verts and shifting all the indices would probably be a slow process and a lot more memory intensive
            offset, count = index_buffer.get_vertex_range(segment)
            self.vertex_buffer = vertex_buffer.slice(offset, count)
            self.index_buffer = index_buffer.relative_slice(segment)
            self.triangle_sets = [(segment.material_index, list(self.index_buffer.get_triangles(0, -1)))]

    def chain_triangles(self) -> Iterator[Triangle]:
        ''' Iterates over all triangles across all material ids '''
        if len(self.triangle_sets) == 1:
            return self.triangle_sets[0][1]
        triangles = (t[1] for t in self.triangle_sets)
        return itertools.chain(*triangles)


class ViewportInterface(Generic[TMaterial, TCollection, TMatrix, TModelState, TRegionGroup]):

    def init_scene(self, scene: Scene, options: ImportOptions) -> None:
        ...

    def pre_import(self, root_collection: TCollection):
        ...

    def post_import(self):
        ...

    def init_materials(self) -> None:
        ...

    def create_material(self, material: Material) -> TMaterial:
        ...

    def set_materials(self, materials: List[TMaterial]) -> None:
        ...

    def create_collection(self, display_name: str, parent: TCollection) -> TCollection:
        ...

    def identity_transform(self) -> TMatrix:
        ...

    def invert_transform(self, transform: TMatrix) -> TMatrix:
        ...

    def multiply_transform(self, a: TMatrix, b: TMatrix) -> TMatrix:
        ...

    def create_transform(self, transform: Matrix4x4, bone_mode: bool = False) -> TMatrix:
        ...

    def init_model(self, model: Model, filter: ModelFilter, collection: TCollection, display_name: str) -> TModelState:
        ...

    def finish_model(self, model_state: TModelState) -> None:
        ...

    def apply_transform(self, model_state: TModelState, coord_sys: TMatrix, world_transform: TMatrix) -> None:
        ...

    def create_bones(self, model_state: TModelState) -> None:
        ...

    def create_markers(self, model_state: TModelState) -> None:
        ...

    def create_region(self, model_state: TModelState, region: ModelRegion, display_name: str) -> TRegionGroup:
        ...

    def build_mesh(self, model_state: TModelState, permutation: ModelPermutation, region_group: TRegionGroup, world_transform: TMatrix, mesh_params: MeshParams) -> None:
        ...
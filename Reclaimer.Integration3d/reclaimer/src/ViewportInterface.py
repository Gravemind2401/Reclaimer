from typing import TypeVar, Generic, Tuple, List

from .ImportOptions import *
from .SceneFilter import *
from .Scene import *
from .Model import *
from .Material import *
from .Types import *
from .Progress import *

__all__ = [
    'MeshKey',
    'ModelState',
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


class ViewportInterface(Generic[TMaterial, TCollection, TMatrix, TModelState, TRegionGroup]):

    def init_scene(self, scene: Scene, options: ImportOptions) -> None:
        ...

    def init_materials(self) -> None:
        ...

    def create_material(self, material: Material) -> TMaterial:
        ...

    def set_materials(self, materials: List[TMaterial]) -> None:
        ...

    def create_collection(self, display_name: str, parent: TCollection) -> TCollection:
        ...

    def create_transform(self, transform: Matrix4x4, bone_mode: bool = False) -> TMatrix:
        ...

    def init_model(self, model: Model, filter: ModelFilter, collection: TCollection, display_name: str) -> TModelState:
        ...

    def apply_transform(self, model_state: TModelState, transform: TMatrix) -> None:
        ...

    def create_bones(self, model_state: TModelState) -> None:
        ...

    def create_markers(self, model_state: TModelState) -> None:
        ...

    def create_region(self, model_state: TModelState, region: ModelRegion, display_name: str) -> TRegionGroup:
        ...

    def build_mesh(self, model_state: TModelState, region_group: TRegionGroup, transform: TMatrix, mesh: Mesh, mesh_key: MeshKey, display_name: str) -> None:
        ...
from typing import List, Iterator, Optional

from .Scene import *
from .Model import *
from .Types import *

__all__ = [
    'IFilterNode',
    'FilterGroup',
    'SceneFilter',
    'ModelFilter',
    'RegionFilter',
    'PermutationFilter'
]


class IFilterNode:
    _node_type: str = None
    label: str = None
    selected: bool = True

    @property
    def node_type(self) -> str:
        return self._node_type

    def __str__(self) -> str:
        return self.label

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class FilterGroup(IFilterNode):
    _node_type: str = 'Group'
    path: str = None
    groups: List['FilterGroup']
    models: List['ModelFilter']

    def __init__(self, scene: Scene, scene_group: SceneGroup, parent_group: Optional['FilterGroup'] = None):
        self.label = scene_group.name
        self.path = f'{parent_group.path}\{self.label}' if parent_group else '.'
        self.groups = [FilterGroup(scene, g, self) for g in scene_group.child_groups]
        self.models = list(self._get_models(scene, scene_group))

    def _get_models(self, scene: Scene, scene_group: SceneGroup) -> Iterator['ModelFilter']:
        for o in scene_group.child_objects:
            placement = None
            if type(o) == Placement:
                placement = o
                o = o.object
            if type(o) == ModelRef:
                o = scene.model_pool[o.model_index]
            if type(o) == Model:
                yield ModelFilter(o, placement)

    def selected_groups(self) -> Iterator['FilterGroup']:
        for g in self.groups:
            if g.selected:
                yield g

    def selected_models(self) -> Iterator['ModelFilter']:
        for m in self.models:
            if m.selected:
                yield m


class SceneFilter(FilterGroup):
    _node_type: str = 'Scene'
    _scene: Scene

    def __init__(self, scene: Scene):
        super().__init__(scene, scene.root_node)
        self._scene = scene
        self.label = scene.name


class ModelFilter(IFilterNode):
    _node_type: str = 'Model'
    _model: Model
    _placement: Placement
    regions: List['RegionFilter']

    def __init__(self, model: Model, placement: Placement = None):
        self._model = model
        self._placement = placement
        self.label = model.name
        self.regions = [RegionFilter(r) for r in model.regions]

        if placement:
            self._node_type = 'Placement'
            if placement.name:
                self.label = placement.name

    @property
    def transform(self) -> Matrix4x4:
        return self._placement.transform if self._placement else Matrix4x4_IDENTITY

    def selected_regions(self) -> Iterator['RegionFilter']:
        for r in self.regions:
            if r.selected:
                yield r


class RegionFilter(IFilterNode):
    _node_type: str = 'Region'
    _region: ModelRegion
    permutations: List['PermutationFilter']

    def __init__(self, region: ModelRegion):
        self._region = region
        self.label = region.name
        self.permutations = [PermutationFilter(p) for p in region.permutations]

    def selected_permutations(self) -> Iterator['PermutationFilter']:
        for p in self.permutations:
            if p.selected:
                yield p


class PermutationFilter(IFilterNode):
    _node_type: str = 'Permutation'
    _permutation: ModelPermutation

    def __init__(self, permutation: ModelPermutation):
        self._permutation = permutation
        self.label = permutation.name
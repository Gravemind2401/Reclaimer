from enum import Enum
from typing import List, Dict, Iterator, Optional, Tuple, Union

from .Scene import *
from .Model import *
from .Material import *
from .Types import *

__all__ = [
    'IFilterNode',
    'FilterGroup',
    'SceneFilter',
    'ModelFilter',
    'RegionFilter',
    'PermutationFilter'
]


class CheckState(Enum):
    UNCHECKED = 0
    PARTIAL = 1
    CHECKED = 2


class IFilterNode:
    _node_type: str = None
    _parent: 'IFilterNode' = None
    label: str = None
    state: CheckState = CheckState.CHECKED

    @property
    def node_type(self) -> str:
        return self._node_type

    @property
    def selected(self) -> bool:
        return self.state != CheckState.UNCHECKED

    def __init__(self, parent: 'IFilterNode'):
        self._parent = parent

    def enumerate_children(self) -> Iterator['IFilterNode']:
        yield from ()

    def _enumerate_descendants(self) -> Iterator['IFilterNode']:
        for c in self.enumerate_children():
            yield c
            yield from c.enumerate_children()

    def _refresh_state(self):
        ''' Refresh state of self and all anscestors to reflect states of children '''

        states = set(c.state for c in self.enumerate_children())
        if len(states) == 0:
            return # no children
        elif len(states) > 1:
            self.state = CheckState.PARTIAL
        else:
            self.state = states.pop()

        if self._parent:
            self._parent._refresh_state()

    def toggle(self, state: Optional[Union[CheckState, int]] = None):
        ''' Toggle state of self and all descendants '''

        if type(state) == int:
            state = CheckState(state)

        # use state param if provided, else invert state
        if state != None:
            self.state = state
        else:
            self.state = CheckState.CHECKED if self.state != CheckState.CHECKED else CheckState.UNCHECKED

        # set all descendants to match the new state
        for c in self._enumerate_descendants():
            c.state = self.state

        if self._parent:
            self._parent._refresh_state()

    def __str__(self) -> str:
        return self.label

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class FilterGroup(IFilterNode):
    _node_type: str = 'Group'
    path: str = None
    groups: List['FilterGroup']
    models: List['ModelFilter']

    def __init__(self, parent: IFilterNode, scene: Scene, scene_group: SceneGroup, parent_group: Optional['FilterGroup'] = None):
        super().__init__(parent)
        self.label = scene_group.name
        self.path = f'{parent_group.path}\{self.label}' if parent_group else '.'
        self.groups = [FilterGroup(self, scene, g, self) for g in scene_group.child_groups]
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
                yield ModelFilter(self, o, placement)

    def enumerate_children(self) -> Iterator[IFilterNode]:
        yield from self.groups
        yield from self.models

    def selected_groups(self) -> Iterator['FilterGroup']:
        for g in self.groups:
            if g.selected:
                yield g

    def _selected_groups_recursive(self) -> Iterator['FilterGroup']:
        for g in self.selected_groups():
            yield g
            yield from g._selected_groups_recursive()

    def selected_models(self) -> Iterator['ModelFilter']:
        for m in self.models:
            if m.selected:
                yield m

    def _selected_models_recursive(self) -> Iterator['ModelFilter']:
        for g in self._selected_groups_recursive():
            yield from g.selected_models()
        yield from self.selected_models()


class SceneFilter(FilterGroup):
    _node_type: str = 'Scene'
    _scene: Scene

    def __init__(self, scene: Scene):
        super().__init__(None, scene, scene.root_node)
        self._scene = scene
        self.label = scene.name

    def selected_materials(self) -> Iterator[Tuple[int, Material]]:
        ''' Iterates over tuples of (material_index, material) only for materials in use by selected meshes '''

        material_ids = set()
        for model in self._selected_models_recursive():
            for region in model.selected_regions():
                for perm in region.selected_permutations():
                    for mesh in perm._permutation.get_meshes(model._model):
                        for segment in mesh.segments:
                            material_ids.add(segment.material_index)

        for id in material_ids:
            yield (id, self._scene.material_pool[id])

    def selected_textures(self) -> Iterator[Tuple[int, Texture]]:
        ''' Iterates over tuples of (texture_index, texture) only for textures in use by selected meshes '''

        texture_ids = set()
        for _, mat in self.selected_materials():
            for t in mat.texture_mappings:
                texture_ids.add(t.texture_index)

        for id in texture_ids:
            yield (id, self._scene.texture_pool[id])


class ModelFilter(IFilterNode):
    _node_type: str = 'Model'
    _model: Model
    _placement: Placement
    regions: List['RegionFilter']

    @property
    def transform(self) -> Matrix4x4:
        return self._placement.transform if self._placement else Matrix4x4_IDENTITY

    def __init__(self, parent: IFilterNode, model: Model, placement: Placement = None):
        super().__init__(parent)
        self._model = model
        self._placement = placement
        self.label = model.name
        self.regions = [RegionFilter(self, r) for r in model.regions]

        if placement:
            self._node_type = 'Placement'
            if placement.name:
                self.label = placement.name

    def enumerate_children(self) -> Iterator[IFilterNode]:
        yield from self.regions

    def selected_regions(self) -> Iterator['RegionFilter']:
        for r in self.regions:
            if r.selected:
                yield r


class RegionFilter(IFilterNode):
    _node_type: str = 'Region'
    _region: ModelRegion
    permutations: List['PermutationFilter']

    def __init__(self, parent: IFilterNode, region: ModelRegion):
        super().__init__(parent)
        self._region = region
        self.label = region.name
        self.permutations = [PermutationFilter(self, p) for p in region.permutations]

    def enumerate_children(self) -> Iterator[IFilterNode]:
        yield from self.permutations

    def selected_permutations(self) -> Iterator['PermutationFilter']:
        for p in self.permutations:
            if p.selected:
                yield p


class PermutationFilter(IFilterNode):
    _node_type: str = 'Permutation'
    _permutation: ModelPermutation

    def __init__(self, parent: IFilterNode, permutation: ModelPermutation):
        super().__init__(parent)
        self._permutation = permutation
        self.label = permutation.name
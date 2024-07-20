from enum import Enum
from typing import cast
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

    def _refresh_state(self, bubble: bool = True):
        ''' Refresh state of self and all anscestors to reflect states of children '''

        states = set(c.state for c in self.enumerate_children())
        if len(states) == 0:
            return # no children
        elif len(states) > 1:
            self.state = CheckState.PARTIAL
        else:
            self.state = states.pop()

        if bubble and self._parent:
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
        self._push_state()

        if self._parent:
            self._parent._refresh_state()

    def _push_state(self):
        ''' Push current state to all descendants '''

        for c in self.enumerate_children():
            c.state = self.state
            c._push_state()

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
        for placement in scene_group.child_objects:
            # TODO: other object types in future
            if type(placement.object) == Model:
                yield ModelFilter(self, placement.object, placement)

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

    def count_objects(self) -> int:
        return sum(1 for _ in self._selected_models_recursive())

    def count_materials(self) -> int:
        return sum(1 for _ in self.selected_materials())

    def count_meshes(self) -> int:
        count = 0
        for m in self._selected_models_recursive():
            for r in m.selected_regions():
                for p in r.selected_permutations():
                    count = count + p._permutation.mesh_count
        return count


class ModelFilter(IFilterNode):
    _node_type: str = 'Model'
    _model: Model
    _placement: Placement
    regions: List['RegionFilter']
    permutation_sets: List['PermutationSetFilter']

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

        sets: Dict[str, List] = {}
        for r in self.regions:
            for p in r.permutations:
                if p.label not in sets.keys():
                    sets[p.label] = []
                sets[p.label].append(p)

        self.permutation_sets = [PermutationSetFilter(self, sets[k]) for k in sorted(sets.keys())]

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

    def _push_state(self):
        model = cast(ModelFilter, self._parent._parent)
        for s in model.permutation_sets:
            if self in s._permutations:
                s._refresh_state()
                break


class PermutationSetFilter(IFilterNode):
    _node_type: str = 'Permutation Set'
    _permutations: List[PermutationFilter]

    def __init__(self, parent: IFilterNode, permutations: List[PermutationFilter]):
        super().__init__(parent)
        self._permutations = permutations
        self.label = permutations[0].label

    def enumerate_children(self) -> Iterator[IFilterNode]:
        yield from self._permutations

    def toggle(self, state: Optional[Union[CheckState, int]] = None):
        for p in self._permutations:
            p.toggle(state)
        self._refresh_state()

    def _refresh_state(self, bubble: bool = True):
        super()._refresh_state(False)
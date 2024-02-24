from typing import Optional, Any

from .ImportOptions import *
from .Progress import *
from .Scene import *
from .SceneFilter import *
from .ViewportInterface import *

__all__ = [
    'SceneBuilder'
]


class SceneBuilder():
    _interface: ViewportInterface
    _scene: Scene
    _filter: SceneFilter
    _options: ImportOptions
    _progress: ProgressCallback

    def __init__(self, interface: ViewportInterface, scene: Scene, filter: Optional[SceneFilter] = None, options: Optional[ImportOptions] = None, callback: Optional[ProgressCallback] = None):
        if not filter:
            filter = SceneFilter(scene)
        if not options:
            options = ImportOptions()
        if not callback:
            callback = ProgressCallback(filter, options)

        self._interface = interface
        self._scene = scene
        self._filter = filter
        self._options = options
        self._progress = callback

    def create_scene(self):
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress

        print(f'scene name: {scene.name}')
        print(f'scene scale: {scene.unit_scale}')

        interface.init_scene(scene, options)

        self._create_materials()

        # TODO: enforce unique collection names
        root_collection = interface.create_collection(scene.name, None)

        for group in filter.selected_groups():
            if progress.cancel_requested:
                break
            self._create_scene_group(group, root_collection)

        for model in filter.selected_models():
            if progress.cancel_requested:
                break
            self._create_model(model, root_collection)

        progress.complete()

    def _create_materials(self):
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress

        # prefill with None to ensure list has correct number of elements
        result = [None for _ in scene.material_pool]

        if not options.IMPORT_MATERIALS:
            interface.set_materials(result)
            return

        print(f'creating {scene.name}/materials')

        interface.init_materials()

        for i, m in filter.selected_materials():
            print(f'creating material: {m.name}')
            material = interface.create_material(m)
            result[i] = material
            progress.increment_materials()

        interface.set_materials(result)

    def _create_scene_group(self, filter_item: FilterGroup, parent: Any) -> None:
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress

        print(f'creating scene group: {filter_item.path}')

        # TODO: enforce unique collection names
        collection = interface.create_collection(filter_item.label, parent)

        for group in filter_item.selected_groups():
            if progress.cancel_requested:
                break
            self._create_scene_group(group, collection)

        for model in filter_item.selected_models():
            if progress.cancel_requested:
                break
            self._create_model(model, collection)

    def _create_model(self, filter_item: ModelFilter, collection: Any):
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress
        model = filter_item._model

        model_state = interface.init_model(model, filter_item, collection, options.model_name(model))

        if options.IMPORT_BONES and model.bones:
            self._create_bones(model_state)
        if options.IMPORT_MESHES and model.meshes:
            self._create_meshes(model_state)
        if options.IMPORT_MARKERS and model.markers:
            self._create_markers(model_state)

        transform = interface.create_transform(filter_item.transform, True)
        interface.apply_transform(model_state, transform)

        progress.increment_objects()

    def _create_bones(self, model_state: ModelState):
        print(f'creating {model_state.model.name}/bones')
        self._interface.create_bones(model_state)

    def _create_markers(self, model_state: ModelState):
        print(f'creating {model_state.model.name}/markers')
        self._interface.create_markers(model_state)

    def _create_meshes(self, model_state: ModelState):
        interface, scene, options, progress = self._interface, self._scene, self._options, self._progress
        model, filter = model_state.model, model_state.filter

        print(f'creating {model.name}/meshes')

        total_meshes = 0
        for i, rf in enumerate(filter.selected_regions()):
            if progress.cancel_requested:
                break
            r = rf._region
            region_name = f'{model_state.display_name}::{options.region_name(r)}'
            region_group = interface.create_region(model_state, r, region_name)
            for j, pf in enumerate(rf.selected_permutations()):
                if progress.cancel_requested:
                    break
                p = pf._permutation

                transform = interface.create_transform(p.transform)
                for mesh_index in range(p.mesh_index, p.mesh_index + p.mesh_count):
                    mesh = model.meshes[mesh_index]
                    mesh_key = (scene.model_pool.index(model), mesh_index, -1) # TODO: last element reserved for submesh index if mesh splitting enabled

                    print(f'creating mesh {total_meshes:03d}: {model.name}/{r.name}/{p.name}/{mesh_index} [{i:02d}/{j:02d}/{mesh_index:02d}]')

                    interface.build_mesh(model_state, region_group, transform, mesh, mesh_key, options.permutation_name(r, p, mesh_index))
                    progress.increment_meshes()
                    total_meshes += 1
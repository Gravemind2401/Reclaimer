from typing import Optional, Any, Union
from queue import Queue, LifoQueue as Stack
from time import time
from functools import partial

from .ImportOptions import *
from .Progress import *
from .Scene import *
from .SceneFilter import *
from .ViewportInterface import *

__all__ = [
    'SceneBuilder',
    'TaskQueue'
]


class TaskQueue():
    stack: Stack
    queue: Queue
    error: Exception

    def __init__(self, initial: Queue) -> None:
        self.stack = Stack()
        self.queue = initial
        self.error = None

    def finished(self) -> bool:
        return self.error or (self.queue.empty() and self.stack.empty())

    def execute_batch(self, timeout: float = 0.0167):
        t = time()
        while time() - t < timeout:
            self.execute_next()

    def execute_next(self):
        # move to the next queue if necessary
        while self.queue.empty() and not self.stack.empty():
            self.queue = self.stack.get()

        # no work remaining
        if self.finished():
            return

        try:
            # execute the task
            task = self.queue.get()
            result = task()

            # if the task returns another queue, put the current one onto the stack
            # and execute the new queue first before continuing
            if isinstance(result, Queue):
                self.stack.put(self.queue)
                self.queue = result
        except Exception as ex:
            # record the error and purge the queues
            self.error = ex
            while not self.stack.empty():
                self.stack.get()
            while not self.queue.empty():
                self.queue.get()


class SceneBuilder():
    _interface: ViewportInterface
    _scene: Scene
    _filter: SceneFilter
    _options: ImportOptions
    _progress: ProgressCallback
    _start_time: float

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

    def begin_create_scene(self) -> TaskQueue:
        interface, scene, filter, options = self._interface, self._scene, self._filter, self._options

        self._start_time = time()

        print(f'scene name: {scene.name}')
        print(f'scene scale: {scene.unit_scale}')

        interface.init_scene(scene, options)

        # TODO: enforce unique collection names
        root_collection = interface.create_collection(scene.name, None)
        interface.pre_import(root_collection)

        q = Queue()
        q.put(partial(self._create_materials))

        for group in filter.selected_groups():
            q.put(partial(self._create_scene_group, group, root_collection))

        for model in filter.selected_models():
            q.put(partial(self._create_model, model, root_collection))

        return TaskQueue(q)

    def end_create_scene(self):
        self._interface.post_import()
        self._progress.complete()
        end_time = time()
        seconds = round(end_time - self._start_time, 3)
        print(f'finished in {seconds} seconds')

    def _create_materials(self) -> Union[None, Queue]:
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress

        # prefill with None to ensure list has correct number of elements
        result = [None for _ in scene.material_pool]

        if not options.IMPORT_MATERIALS:
            interface.set_materials(result)
            return

        print(f'creating {scene.name}/materials')

        q = Queue()
        q.put(partial(interface.init_materials))

        for i, m in filter.selected_materials():
            def create_material(mat, idx):
                print(f'creating material: {mat.name}')
                material = interface.create_material(mat)
                result[idx] = material
                progress.increment_materials()
            q.put(partial(create_material, m, i))

        q.put(partial(interface.set_materials, result))
        return q

    def _create_scene_group(self, filter_item: FilterGroup, parent: Any) -> Queue:
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress

        print(f'creating scene group: {filter_item.path}')

        # TODO: enforce unique collection names
        collection = interface.create_collection(filter_item.label, parent)

        q = Queue()

        for group in filter_item.selected_groups():
            q.put(partial(self._create_scene_group, group, collection))

        for model in filter_item.selected_models():
            q.put(partial(self._create_model, model, collection))

        return q

    def _create_model(self, filter_item: ModelFilter, collection: Any) -> Queue:
        interface, scene, filter, options, progress = self._interface, self._scene, self._filter, self._options, self._progress
        model = filter_item._model

        model_state = interface.init_model(model, filter_item, collection, options.model_name(model))

        q = Queue()

        if options.IMPORT_BONES and model.bones:
            q.put(partial(self._create_bones, model_state))
        if options.IMPORT_MESHES and model.meshes:
            q.put(partial(self._create_meshes, model_state))
        if options.IMPORT_MARKERS and model.markers:
            q.put(partial(self._create_markers, model_state))

        def transform_func():
            target_sys = interface.identity_transform()
            source_sys = interface.create_transform(self._scene.world_matrix, True)
            world_transform = interface.create_transform(filter_item.transform, True)

            conversion = interface.multiply_transform(interface.invert_transform(source_sys), target_sys)
            final_transform = interface.multiply_transform(conversion, world_transform)

            interface.apply_transform(model_state, final_transform)

        q.put(partial(transform_func))
        q.put(partial(progress.increment_objects))

        return q

    def _create_bones(self, model_state: ModelState):
        print(f'creating {model_state.model.name}/bones')
        self._interface.create_bones(model_state)

    def _create_markers(self, model_state: ModelState):
        print(f'creating {model_state.model.name}/markers')
        self._interface.create_markers(model_state)

    def _create_meshes(self, model_state: ModelState) -> Queue:
        interface, scene, options, progress = self._interface, self._scene, self._options, self._progress
        model, filter = model_state.model, model_state.filter

        print(f'creating {model.name}/meshes')

        q = Queue()

        total_meshes = 0
        for i, rf in enumerate(filter.selected_regions()):
            r = rf._region
            region_name = f'{model_state.display_name}::{options.region_name(r)}'
            region_group = interface.create_region(model_state, r, region_name)
            for j, pf in enumerate(rf.selected_permutations()):
                p = pf._permutation

                world_transform = interface.create_transform(p.transform)
                for mesh_index in range(p.mesh_index, p.mesh_index + p.mesh_count):
                    mesh = model.meshes[mesh_index]
                    mesh_key = (scene.model_pool.index(model), mesh_index, -1) # TODO: last element reserved for submesh index if mesh splitting enabled
                    message = f'creating mesh {total_meshes:03d}: {model.name}/{r.name}/{p.name}/{mesh_index} [{i:02d}/{j:02d}/{mesh_index:02d}]'
                    mesh_name = options.permutation_name(r, p, mesh_index)

                    def mesh_func(message, model_state, region_group, transform, mesh, mesh_key, mesh_name):
                        print(message)
                        interface.build_mesh(model_state, region_group, transform, mesh, mesh_key, mesh_name)
                        progress.increment_meshes()

                    q.put(partial(mesh_func, message, model_state, region_group, world_transform, mesh, mesh_key, mesh_name))
                    total_meshes += 1

        return q
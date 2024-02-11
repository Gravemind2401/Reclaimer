from ..src.SceneFilter import SceneFilter
from ..src.ImportOptions import ImportOptions


class ProgressCallback:
    cancel_requested: bool = False

    object_count: int = 0
    material_count: int = 0
    mesh_count: int = 0

    object_progress: int = 0
    material_progress: int = 0
    mesh_progress: int = 0

    def __init__(self, filter: SceneFilter, options: ImportOptions):
        self.object_count = filter.count_objects()
        if options.IMPORT_MATERIALS:
            self.material_count = filter.count_materials()
        if options.IMPORT_MESHES:
            self.mesh_count = filter.count_meshes()

    def increment_objects(self):
        self.object_progress = self.object_progress + 1
        self._refresh()

    def increment_materials(self):
        self.material_progress = self.material_progress + 1
        self._refresh()

    def increment_meshes(self):
        self.mesh_progress = self.mesh_progress + 1
        self._refresh()

    def complete(self):
        ''' Override in derived class '''

    def _refresh(self):
        ''' Override in derived class '''
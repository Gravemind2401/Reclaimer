from ..src.SceneFilter import SceneFilter
from ..src.ImportOptions import ImportOptions


class ProgressCallback:
    cancel_requested: bool = False

    material_count: int = 0
    mesh_count: int = 0
    object_count: int = 0

    material_progress: int = 0
    mesh_progress: int = 0
    object_progress: int = 0

    @property
    def material_percent(self) -> float:
        return int(self.material_progress / self.material_count * 100) / 100

    @property
    def mesh_percent(self) -> float:
        return int(self.mesh_progress / self.mesh_count * 100) / 100

    @property
    def object_percent(self) -> float:
        return int(self.object_progress / self.object_count * 100) / 100

    def __init__(self, filter: SceneFilter, options: ImportOptions):
        self.object_count = filter.count_objects()
        if options.IMPORT_MATERIALS:
            self.material_count = filter.count_materials()
        if options.IMPORT_MESHES:
            self.mesh_count = filter.count_meshes()

    def increment_materials(self):
        self.material_progress = self.material_progress + 1
        self._refresh()

    def increment_meshes(self):
        self.mesh_progress = self.mesh_progress + 1
        self._refresh()

    def increment_objects(self):
        self.object_progress = self.object_progress + 1
        self._refresh()

    def complete(self):
        ''' Override in derived class '''

    def _refresh(self):
        ''' Override in derived class '''
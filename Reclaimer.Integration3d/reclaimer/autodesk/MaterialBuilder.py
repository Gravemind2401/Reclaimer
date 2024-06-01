from typing import Dict

from pymxs import runtime as rt

from ..src.SceneReader import *
from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *

__all__ = [
    'MaterialBuilder'
]


class MaterialBuilder:
    _scene: Scene
    _options: ImportOptions
    _bitmap_lookup: Dict[int, rt.OSLMap]
    _material_lookup: Dict[int, rt.PhysicalMaterial]
    _multi_lookup: Dict[int, rt.MultiMaterial]

    def __init__(self, scene: Scene, options: ImportOptions):
        self._scene = scene
        self._options = options
        self._bitmap_lookup = dict()
        self._material_lookup = dict()
        self._multi_lookup = dict()

    def create_material(self, mat: Material) -> rt.Material:
        id = self._scene.material_pool.index(mat)
        result = self._material_lookup.get(id, None)
        if result:
            return result

        result = rt.PhysicalMaterial()
        result.name = mat.name

        for t in mat.texture_mappings:
            if t.texture_usage == TEXTURE_USAGE.DIFFUSE:
                tex = self._scene.texture_pool[t.texture_index]
                result.base_color_map = rt.BitmapTexture(fileName=self._options.texture_path(tex))
                break

        self._material_lookup[id] = result

        return result

    def create_multi_material(self, model: Model) -> rt.Material:
        model_key = self._scene.model_pool.index(model)
        multi = self._multi_lookup.get(model_key, None)
        if multi:
            return multi

        def get_material_ids():
            for m in model.meshes:
                for s in m.segments:
                    yield s.material_index + 1 # adjust for 1-based max arrays

        material_ids = sorted(filter(lambda id: id > 0, set(get_material_ids())))
        material_count = len(material_ids)
        material_names = [self._scene.material_pool[id - 1].name for id in material_ids]

        next_multi = len(self._multi_lookup) # apparently MeditMaterials is zero-based, unlike most other max arrays
        multi = rt.MultiMaterial(numSubs=material_count, names=material_names, materialIdList=material_ids)
        multi.name = model.name

        for i, id in enumerate(material_ids):
            #another zero-based array because who needs consistency
            multi.materialList[i] = self._material_lookup.get(id - 1, None)

        rt.MeditMaterials[next_multi] = self._multi_lookup[model_key] = multi
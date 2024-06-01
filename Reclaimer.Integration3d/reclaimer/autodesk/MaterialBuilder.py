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


class BitmapLookupOutputs:
    RGB = 1
    RED = 2
    GREEN = 3
    BLUE = 4
    ALPHA = 5
    LUMINANCE = 6
    AVERAGE = 7


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

        def create_texmap(map: TextureMapping) -> rt.TextureMap:
            #TODO: create a lookup of unique OSLBitmap2 instances instead of creating a new one each time
            tex = self._scene.texture_pool[map.texture_index]
            selector = rt.MultiOutputChannelTexmapToTexmap()

            #Osl_OSLBitmap2 is called "BitmapLookup" in the UI
            bitm = selector.sourceMap = rt.Osl_OSLBitmap2(fileName=self._options.texture_path(tex))
            bitm.autoGamma = 0
            bitm.manualGamma = tex.gamma

            if map.channel_mask == ChannelFlags.RED:
                selector.outputChannelIndex = BitmapLookupOutputs.RED
            elif map.channel_mask == ChannelFlags.GREEN:
                selector.outputChannelIndex = BitmapLookupOutputs.GREEN
            elif map.channel_mask == ChannelFlags.BLUE:
                selector.outputChannelIndex = BitmapLookupOutputs.BLUE
            elif map.channel_mask == ChannelFlags.ALPHA:
                selector.outputChannelIndex = BitmapLookupOutputs.ALPHA
            else:
                selector.outputChannelIndex = BitmapLookupOutputs.RGB

            return selector

        for t in mat.texture_mappings:
            if t.texture_usage == TEXTURE_USAGE.DIFFUSE:
                result.base_color_map = create_texmap(t)
                break

        for t in mat.texture_mappings:
            if t.texture_usage == TEXTURE_USAGE.NORMAL:
                norm = rt.Normal_Bump()
                norm.normal_map = create_texmap(t)
                result.bump_map = norm
                break

        for t in mat.texture_mappings:
            if t.texture_usage == TEXTURE_USAGE.SPECULAR:
                result.reflectivity_map = create_texmap(t)
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
        return multi

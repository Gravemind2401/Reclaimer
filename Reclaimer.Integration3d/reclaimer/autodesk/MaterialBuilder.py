from typing import Dict, List

from pymxs import runtime as rt

from .Utils import *
from .. import autodesk
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
    _material_lookup: Dict[int, rt.PhysicalMaterial]
    _multi_lookup: Dict[int, rt.MultiMaterial]

    def __init__(self, scene: Scene, options: ImportOptions):
        self._scene = scene
        self._options = options
        self._material_lookup = dict()
        self._multi_lookup = dict()

    def create_material(self, mat: Material) -> rt.Material:
        id = self._scene.material_pool.index(mat)
        result = self._material_lookup.get(id, None)
        if result:
            return result

        bitmap_lookup = dict()

        usage_lookup: Dict[str, List[TextureMapping]] = {
            TEXTURE_USAGE.BLEND: [],
            TEXTURE_USAGE.DIFFUSE: [],
            TEXTURE_USAGE.NORMAL: [],
            TEXTURE_USAGE.HEIGHT: [],
            TEXTURE_USAGE.SPECULAR: [],
            TEXTURE_USAGE.TRANSPARENCY: [],
            TEXTURE_USAGE.COLOR_CHANGE: []
        }

        for t in mat.texture_mappings:
            if t.texture_usage in usage_lookup:
                usage_lookup[t.texture_usage].append(t)

        result = rt.PhysicalMaterial()
        result.name = mat.name

        def create_bitmap(src: TextureMapping) -> rt.Osl_OSLBitmap2:
            bitm = bitmap_lookup.get(src.texture_index, None)
            if bitm:
                return bitm

            tex = self._scene.texture_pool[src.texture_index]

            #Osl_OSLBitmap2 is called "BitmapLookup" in the UI
            bitm = rt.Osl_OSLBitmap2(fileName=self._options.texture_path(tex))
            bitm.name = tex.name.split('\\')[-1]
            bitm.autoGamma = False
            bitm.manualGamma = tex.gamma

            if src.tiling != (1, 1):
                bitm.Pos_map = uvw = rt.Osl_UVWTransform()
                uvw.Tiling = rt.Point3(src.tiling[0], src.tiling[1], 1)

            bitmap_lookup[src.texture_index] = bitm
            return bitm

        def create_texmap(map: TextureMapping) -> rt.TextureMap:
            selector = rt.MultiOutputChannelTexmapToTexmap()
            selector.sourceMap = create_bitmap(map)

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

        if usage_lookup[TEXTURE_USAGE.BLEND]:
            blend_texmap = usage_lookup[TEXTURE_USAGE.BLEND][0]

            def create_blend(mappings: List[TextureMapping]) -> rt.TextureMap:
                selector = rt.MultiOutputChannelTexmapToTexmap()
                blend = selector.sourceMap = rt.OSLMap(name='Blend Map')
                blend.oslPath = autodesk.resource('OSLBlendMap.osl')
                #blend.oslAutoUpdate = True

                mask_bitm = create_bitmap(blend_texmap)

                blend.Mask1_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.RED)
                blend.Mask2_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.GREEN)
                blend.Mask3_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.BLUE)
                blend.Mask4_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.ALPHA)

                for m in mappings:
                    input_tex = create_texmap(m)
                    if m.blend_channel == ChannelFlags.RED:
                        blend.Input1_map = input_tex
                    elif m.blend_channel == ChannelFlags.GREEN:
                        blend.Input2_map = input_tex
                    elif m.blend_channel == ChannelFlags.BLUE:
                        blend.Input3_map = input_tex
                    elif m.blend_channel == ChannelFlags.ALPHA:
                        blend.Input4_map = input_tex

                return selector

            diffuse_maps = usage_lookup[TEXTURE_USAGE.DIFFUSE]
            if diffuse_maps:
                result.base_color_map = create_blend(diffuse_maps)

            normal_maps = usage_lookup[TEXTURE_USAGE.NORMAL]
            if normal_maps:
                result.bump_map = norm = rt.Normal_Bump()
                norm.normal_map = create_blend(normal_maps)

            spec_maps = usage_lookup[TEXTURE_USAGE.SPECULAR]
            if spec_maps:
                result.reflectivity_map = create_blend(spec_maps)
        else:
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

            for t in mat.tints:
                if t.tint_usage == TINT_USAGE.EMISSION:
                    result.emit_color = rt.Color(t.tint_color[0], t.tint_color[1], t.tint_color[2])
                    break

            for t in mat.texture_mappings:
                if t.texture_usage == TEXTURE_USAGE.EMISSION:
                    result.emission_map = create_texmap(t)
                    break

            for t in mat.texture_mappings:
                if t.texture_usage == TEXTURE_USAGE.COLOR_CHANGE:
                    selector = rt.MultiOutputChannelTexmapToTexmap()
                    ccmap = selector.sourceMap = rt.OSLMap(name='Color Change Map')
                    ccmap.oslPath = autodesk.resource('OSLColorChangeMap.osl')
                    #ccmap.oslAutoUpdate = True

                    mask_bitm = create_bitmap(t)

                    if t.channel_mask & ChannelFlags.RED:
                        ccmap.Mask1_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.RED)
                    if t.channel_mask & ChannelFlags.GREEN:
                        ccmap.Mask2_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.GREEN)
                    if t.channel_mask & ChannelFlags.BLUE:
                        ccmap.Mask3_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.BLUE)
                    if t.channel_mask & ChannelFlags.ALPHA:
                        ccmap.Mask4_map = rt.MultiOutputChannelTexmapToTexmap(sourceMap=mask_bitm, outputChannelIndex=BitmapLookupOutputs.ALPHA)

                    if result.base_color_map:
                        ccmap.BaseColor_map = result.base_color_map

                    ccmap.Color1 = toColor(self._options.DEFAULTCC_1)
                    ccmap.Color2 = toColor(self._options.DEFAULTCC_2)
                    ccmap.Color3 = toColor(self._options.DEFAULTCC_3)
                    ccmap.Color4 = toColor(self._options.DEFAULTCC_4)

                    result.base_color_map = selector
                    break

            if mat.alpha_mode != ALPHA_MODE.OPAQUE:
                for t in mat.texture_mappings:
                    if t.texture_usage == TEXTURE_USAGE.TRANSPARENCY:
                        result.cutout_map = create_texmap(t)
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

        self._multi_lookup[model_key] = multi

        #sample slots are limited, but set them where possible
        if len(rt.MeditMaterials) > next_multi:
            rt.MeditMaterials[next_multi] = multi

        return multi

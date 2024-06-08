import bpy
from typing import Dict, List, Tuple

from ..src.SceneReader import *
from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Material import *
from ..src.Types import *
from .Compatibility import *
from .CustomShaderNodes import *

__all__ = [
    'MaterialBuilder'
]

__channel_socket_lookup: Dict[ChannelFlags, str] = {
    ChannelFlags.RGB: 'Color',
    ChannelFlags.RED: 'R',
    ChannelFlags.GREEN: 'G',
    ChannelFlags.BLUE: 'B',
    ChannelFlags.ALPHA: 'Alpha'
}

def _create_uvscale_node(material: bpy.types.Material, input: TextureMapping, image_node: bpy.types.Node) -> bpy.types.Node:
    if input.tiling == (1, 1):
        return None

    scale_node = create_group_node(material, 'UV Scale')
    scale_node.inputs['X'].default_value = input.tiling[0]
    scale_node.inputs['Y'].default_value = input.tiling[1]
    material.node_tree.links.new(image_node.inputs['Vector'], scale_node.outputs['Vector'])
    return scale_node

def _get_channel_socket(input: TextureMapping):
    mask = input.channel_mask
    if mask not in __channel_socket_lookup:
        mask = ChannelFlags.RGB
    return __channel_socket_lookup[mask]


class TextureHelper:
    texture_data: Texture = None
    input_data: Dict[str, TextureMapping] = None

    material: bpy.types.Material = None
    frame_node: bpy.types.NodeFrame = None
    texture_node: bpy.types.ShaderNodeTexImage = None
    gamma_node: bpy.types.ShaderNodeGamma = None
    rgb_node: ShaderNodeSeparateColorCompat = None
    scale_node: bpy.types.ShaderNode = None

    @property
    def default_input(self) -> TextureMapping:
        return next(iter(self.input_data.values()))

    def __init__(self, texture: Texture, usages: Dict[str, TextureMapping], material: bpy.types.Material, image: bpy.types.Image):
        self.input_data = usages
        self.texture_data = texture
        self.material = material

        # set up texture node
        self.texture_node = material.node_tree.nodes.new('ShaderNodeTexImage')
        self.texture_node.image = image

        # append frame first so it can be set as parent for future nodes
        self._append_frame_node()
        self._append_scale_node()

        if self.default_input.texture_usage == TEXTURE_USAGE.NORMAL:
            image.alpha_mode = 'NONE'
            image.colorspace_settings.name = 'Non-Color'
        else:
            image.alpha_mode = 'CHANNEL_PACKED'
            if self.texture_data.gamma == GAMMA_PRESET.SRGB:
                image.colorspace_settings.name = 'sRGB'
            else:
                image.colorspace_settings.name = 'Linear' if bpy.app.version[0] < 4 else 'Linear Rec.709'
                if self.texture_data.gamma != GAMMA_PRESET.LINEAR:
                    self._append_gamma_node()

    def _append_frame_node(self):
        self.frame_node = self.material.node_tree.nodes.new('NodeFrame')
        self.frame_node.label = self.default_input.texture_usage
        self.texture_node.parent = self.frame_node

    def _append_scale_node(self):
        self.scale_node = _create_uvscale_node(self.material, self.default_input, self.texture_node)
        if self.scale_node:
            self.scale_node.parent = self.frame_node
            self.scale_node.location = (-200, -100)

    def _append_gamma_node(self):
        self.gamma_node = self.material.node_tree.nodes.new('ShaderNodeGamma')
        self.gamma_node.inputs['Gamma'].default_value = self.texture_data.gamma
        self.gamma_node.parent = self.frame_node
        self.gamma_node.location = (300, 0)
        self.material.node_tree.links.new(self.gamma_node.inputs['Color'], self.texture_node.outputs['Color'])

    def _append_rgb_node(self):
        self.rgb_node = ShaderNodeSeparateColorCompat(self.material.node_tree)
        self.rgb_node.parent = self.frame_node
        self.rgb_node.location = (300, -130)
        self.material.node_tree.links.new(self.rgb_node.inputs['Color'], self.get_output('Color'))

    def get_default_channel_source(self, usage: str) -> ChannelFlags:
        input = self.input_data.get(usage, None) or next(iter(self.input_data.items()))[1]
        return input.channel_mask

    def get_default_output(self, usage: str) -> bpy.types.NodeSocket:
        input = self.input_data.get(usage, None) or next(iter(self.input_data.items()))[1]
        output_name = _get_channel_socket(input)
        return self.get_output(output_name)

    def get_output(self, output_name: str) -> bpy.types.NodeSocket:
        if output_name == 'Alpha':
            return self.texture_node.outputs['Alpha']
        if output_name == 'Color':
            node = self.gamma_node or self.texture_node
            return node.outputs['Color']
        if output_name in ['R', 'G', 'B']:
            if not self.rgb_node:
                self._append_rgb_node()
            return self.rgb_node.outputs[output_name]

    def set_location(self, x: int, y: int = None):
        if type(x) == tuple:
            x, y = x
        frame = self.frame_node
        if not (frame.location[0] or frame.location[1]):
            frame.location = (x, y)


class MaterialBuilder:
    _scene: Scene
    _options: ImportOptions
    _image_lookup: Dict[int, bpy.types.Image]

    def __init__(self, scene: Scene, options: ImportOptions):
        self._scene = scene
        self._options = options
        self._image_lookup = dict()

    def _apply_custom_properties(self, target, source: ICustomProperties):
        if self._options.IMPORT_CUSTOM_PROPS:
            for k, v in source.custom_properties.items():
                target[k] = v

    def create_material(self, mat: Material) -> bpy.types.Material:
        scene, OPTIONS = self._scene, self._options

        result = bpy.data.materials.new(OPTIONS.material_name(mat))
        result.use_nodes = True

        self._apply_custom_properties(result, mat)

        bsdf = result.node_tree.nodes['Principled BSDF']

        usage_lookup: Dict[str, List[TextureHelper]] = {
            TEXTURE_USAGE.BLEND: [],
            TEXTURE_USAGE.DIFFUSE: [],
            TEXTURE_USAGE.NORMAL: [],
            TEXTURE_USAGE.HEIGHT: [],
            TEXTURE_USAGE.SPECULAR: [],
            TEXTURE_USAGE.TRANSPARENCY: [],
            TEXTURE_USAGE.COLOR_CHANGE: []
        }

        blend_input_lookup = {
            ChannelFlags.RED: 'R',
            ChannelFlags.GREEN: 'G',
            ChannelFlags.BLUE: 'B',
            ChannelFlags.ALPHA: 'A'
        }

        #add and multiply not supported anymore?
        alpha_mode_lookup = {
            ALPHA_MODE.CLIP: 'CLIP',
            ALPHA_MODE.ADD: 'BLEND',
            ALPHA_MODE.BLEND: 'BLEND',
            ALPHA_MODE.MULTIPLY: 'BLEND',
            ALPHA_MODE.OPAQUE: 'OPAQUE'
        }

        #duplicate textures for different blend channels, but not within the same blend channel
        for channel in (ChannelFlags.DEFAULT, ChannelFlags.RED, ChannelFlags.GREEN, ChannelFlags.BLUE, ChannelFlags.ALPHA):
            texture_lookup = scene.create_texture_lookup(mat, channel)
            for i, (texture, usages) in texture_lookup.items():
                if not any(u in usage_lookup for u in usages):
                    continue #ignore texture if no supported usages

                img = self._get_image(i)
                helper = TextureHelper(texture, usages, result, img)
                for usage in usages.keys():
                    if usage in usage_lookup:
                        usage_lookup[usage].append(helper)

        diffuse_images = usage_lookup[TEXTURE_USAGE.DIFFUSE]
        normal_images = usage_lookup[TEXTURE_USAGE.NORMAL]
        height_images = usage_lookup[TEXTURE_USAGE.HEIGHT]
        specular_images = usage_lookup[TEXTURE_USAGE.SPECULAR]
        transparency_images = usage_lookup[TEXTURE_USAGE.TRANSPARENCY]
        cc_images = usage_lookup[TEXTURE_USAGE.COLOR_CHANGE]

        if usage_lookup[TEXTURE_USAGE.BLEND]:
            x_position = -1700
            y_position = 1400

            def next_position() -> Tuple[float, float]:
                nonlocal x_position, y_position
                next = (x_position, y_position)
                y_position = y_position - 400
                return next

            blend_helper = usage_lookup[TEXTURE_USAGE.BLEND][0]
            blend_frame = blend_helper.frame_node
            blend_frame.location = (-1000, 100)

            comp_blend = create_group_node(result, 'Composite Blend')
            comp_blend.location = (-400, 200)

            result.node_tree.links.new(comp_blend.inputs['Mask RGB'], blend_helper.get_output('Color'))
            result.node_tree.links.new(comp_blend.inputs['Mask A'], blend_helper.get_output('Alpha'))

            if diffuse_images:
                result.node_tree.links.new(bsdf.inputs['Base Color'], comp_blend.outputs['Color'])
                for helper in diffuse_images:
                    input = helper.default_input
                    helper.set_location(next_position())
                    if input.blend_channel in blend_input_lookup:
                        channel = blend_input_lookup[input.blend_channel]
                        result.node_tree.links.new(comp_blend.inputs[f'{channel} Color'], helper.get_default_output(TEXTURE_USAGE.DIFFUSE))

            if normal_images:
                result.node_tree.links.new(bsdf.inputs['Normal'], comp_blend.outputs['Normal'])
                for helper in normal_images:
                    input = helper.default_input
                    helper.set_location(next_position())
                    if input.blend_channel in blend_input_lookup:
                        channel = blend_input_lookup[input.blend_channel]
                        result.node_tree.links.new(comp_blend.inputs[f'{channel} Normal'], helper.get_default_output(TEXTURE_USAGE.NORMAL))

            if specular_images:
                result.node_tree.links.new(bsdf.inputs[SPECULAR_SOCKET_COMPAT], comp_blend.outputs['Specular'])
                for helper in specular_images:
                    input = helper.default_input
                    helper.set_location(next_position())
                    if input.blend_channel in blend_input_lookup:
                        channel = blend_input_lookup[input.blend_channel]
                        result.node_tree.links.new(comp_blend.inputs[f'{channel} Specular'], helper.get_default_output(TEXTURE_USAGE.SPECULAR))
        else:
            # should only ever be max 1 of each input type
            if diffuse_images:
                helper = diffuse_images[0]
                helper.set_location(-700, 300)
                result.node_tree.links.new(bsdf.inputs['Base Color'], helper.get_default_output(TEXTURE_USAGE.DIFFUSE))
            if normal_images:
                helper = normal_images[0]
                helper.set_location(-600, -100)
                normal_node = create_group_node(result, 'DX Normal Map')
                normal_node.location = (-250, -150)
                result.node_tree.links.new(normal_node.inputs['Color'], helper.get_default_output(TEXTURE_USAGE.NORMAL))
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])
            if height_images:
                helper = height_images[0]
                helper.set_location(-600, -100)
                height_node = result.node_tree.nodes.new('ShaderNodeBump')
                height_node.inputs['Strength'].default_value = 1
                height_node.inputs['Distance'].default_value = 0.04
                height_node.location = (-250, -150)
                result.node_tree.links.new(height_node.inputs['Height'], helper.get_default_output(TEXTURE_USAGE.HEIGHT))
                result.node_tree.links.new(bsdf.inputs['Normal'], height_node.outputs['Normal'])
            if specular_images:
                helper = specular_images[0]
                helper.set_location(-1200, -50)
                result.node_tree.links.new(bsdf.inputs[SPECULAR_SOCKET_COMPAT], helper.get_default_output(TEXTURE_USAGE.SPECULAR))
            if transparency_images:
                result.blend_method = alpha_mode_lookup.get(mat.alpha_mode, 'OPAQUE')
                helper = transparency_images[0]
                helper.set_location(-1000, -500)
                result.node_tree.links.new(bsdf.inputs['Alpha'], helper.get_default_output(TEXTURE_USAGE.TRANSPARENCY))
            if cc_images and diffuse_images:
                helper = cc_images[0]
                helper.set_location(-750, 700)
                cc_node = create_group_node(result, 'Color Change')
                cc_node.location = (-200, 600)

                flags = helper.get_default_channel_source(TEXTURE_USAGE.COLOR_CHANGE)
                if flags == ChannelFlags.DEFAULT or flags == ChannelFlags.RGB:
                    result.node_tree.links.new(cc_node.inputs['Primary Mask'], helper.get_output('R'))
                    result.node_tree.links.new(cc_node.inputs['Secondary Mask'], helper.get_output('G'))
                    result.node_tree.links.new(cc_node.inputs['Tertiary Mask'], helper.get_output('B'))
                else:
                    result.node_tree.links.new(cc_node.inputs['Primary Mask'], helper.get_default_output(TEXTURE_USAGE.COLOR_CHANGE))

                result.node_tree.links.new(cc_node.inputs['Base Color'], diffuse_images[0].get_default_output(TEXTURE_USAGE.DIFFUSE))
                result.node_tree.links.new(bsdf.inputs['Base Color'], cc_node.outputs['Color'])

        if mat.tints:
            # add RGB nodes with tint color values (not connected to anything yet)
            tint_frame = result.node_tree.nodes.new('NodeFrame')
            tint_frame.label = 'Tints'
            tint_frame.location = (-400, 600)

            for i, tint in enumerate(mat.tints):
                tint_node: bpy.types.ShaderNodeRGB = result.node_tree.nodes.new('ShaderNodeRGB')
                tint_node.outputs[0].default_value = tuple(c / 255.0 for c in tint.tint_color)
                tint_node.label = tint.tint_usage
                tint_node.parent = tint_frame
                tint_node.location = (200 * i, 0)

        return result

    def _get_image(self, index: int) -> bpy.types.Image:
        scene, OPTIONS = self._scene, self._options

        # ensure each image is only loaded once, regardless of how many materials it gets used in
        if index not in self._image_lookup:
            src = scene.texture_pool[index]
            if src.size > 0:
                print(f'loading embedded texture: {src.name} @ {src.address}')
                pixel_data = SceneReader.read_texture(scene, src)
                print(f'>>> {len(pixel_data)} bytes loaded')

                # create a new empty image and pack it with the embedded pixel data
                img = bpy.data.images.new(name=src.name, width=1, height=1)
                img.pack(data=pixel_data, data_len=src.size)
                img.source = 'FILE' # images.new() initially starts as 'GENERATED'
            else:
                src_path = OPTIONS.texture_path(src)
                print(f'loading texture: {src_path}')
                try:
                    img = bpy.data.images.load(src_path)
                except:
                    print('>>> unable to load image')
                    img = bpy.data.images.new(name=src_path, width=1, height=1)

            self._apply_custom_properties(img, src)
            self._image_lookup[index] = img

        return self._image_lookup[index]
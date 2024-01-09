import bpy
from typing import Dict, List, Tuple

from ..src.SceneReader import *
from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Material import *
from .CustomShaderNodes import *

__all__ = [
    'MaterialBuilder'
]


def _create_uvscale_node(material: bpy.types.Material, input: TextureMapping, image_node: bpy.types.Node) -> bpy.types.Node:
    if input.tiling == (1, 1):
        return None

    scale_node = create_group_node(material, 'UV Scale')
    scale_node.inputs['X'].default_value = input.tiling[0]
    scale_node.inputs['Y'].default_value = input.tiling[1]
    material.node_tree.links.new(image_node.inputs['Vector'], scale_node.outputs['Vector'])
    return scale_node


class TextureHelper:
    input: TextureMapping = None
    material: bpy.types.Material = None
    frame_node: bpy.types.NodeFrame = None
    texture_node: bpy.types.ShaderNodeTexImage = None
    gamma_node: bpy.types.ShaderNodeGamma = None
    scale_node: bpy.types.ShaderNode = None
    color_output: bpy.types.NodeSocket = None
    alpha_output: bpy.types.NodeSocket = None

    def __init__(self, scene: Scene, input: TextureMapping, material: bpy.types.Material, image: bpy.types.Image):
        self.input = input
        self.material = material


        # set up texture node
        self.texture_node = material.node_tree.nodes.new('ShaderNodeTexImage')
        self.texture_node.image = image
        self.color_output = self.texture_node.outputs['Color']
        self.alpha_output = self.texture_node.outputs['Alpha']

        # TODO: set alpha mode depending on channel mask (not 'CHANNEL_PACKED' if transparent)
        # > set alpha mode depending on whether alpha channel included in channel mask

        tex = scene.texture_pool[input.texture_index]

        if input.texture_usage == TEXTURE_USAGE.NORMAL:
            image.alpha_mode = 'NONE'
            image.colorspace_settings.name = 'Non-Color'
        else:
            image.alpha_mode = 'CHANNEL_PACKED'
            if tex.gamma == 2.2:
                image.colorspace_settings.name = 'sRGB'
            else:
                image.colorspace_settings.name = 'Linear'
                if tex.gamma != 1.0:
                    self.gamma_node = material.node_tree.nodes.new('ShaderNodeGamma')
                    self.gamma_node.inputs['Gamma'].default_value = tex.gamma
                    material.node_tree.links.new(self.gamma_node.inputs['Color'], self.texture_node.outputs['Color'])
                    self.color_output = self.gamma_node.outputs['Color']

        # set up frame node
        self.frame_node = self.material.node_tree.nodes.new('NodeFrame')
        self.frame_node.label = self.input.texture_usage
        self.texture_node.parent = self.frame_node

        # set up scale node
        self.scale_node = _create_uvscale_node(material, input, self.texture_node)
        if self.scale_node:
            self.scale_node.parent = self.frame_node
            self.scale_node.location = (-200, -100)
        if self.gamma_node:
            self.gamma_node.parent = self.frame_node
            self.gamma_node.location = (300, 0)


class MaterialBuilder:
    _scene: Scene
    _options: ImportOptions
    _image_lookup: Dict[int, bpy.types.Image]

    def __init__(self, scene: Scene, options: ImportOptions):
        self._scene = scene
        self._options = options
        self._image_lookup = dict()

    def create_material(self, index: int):
        scene, OPTIONS = self._scene, self._options

        mat = scene.material_pool[index]

        result = bpy.data.materials.new(OPTIONS.material_name(mat))
        result.use_nodes = True

        bsdf = result.node_tree.nodes['Principled BSDF']

        usage_lookup: Dict[str, List[TextureHelper]] = {
            TEXTURE_USAGE.BLEND: [],
            TEXTURE_USAGE.DIFFUSE: [],
            TEXTURE_USAGE.NORMAL: []
        }

        blend_input_lookup = {
            ChannelFlags.RED: 'R',
            ChannelFlags.GREEN: 'G',
            ChannelFlags.BLUE: 'B',
            ChannelFlags.ALPHA: 'A'
        }

        for input in mat.texture_mappings:
            if not input.texture_usage in usage_lookup:
                continue

            img = self._get_image(input.texture_index)
            helper = TextureHelper(scene, input, result, img)
            usage_lookup[input.texture_usage].append(helper)

        diffuse_images = usage_lookup[TEXTURE_USAGE.DIFFUSE]
        bump_images = usage_lookup[TEXTURE_USAGE.NORMAL]

        if usage_lookup[TEXTURE_USAGE.BLEND]:
            x_position = -1700
            y_position = 1400

            def next_position() -> Tuple[float, float]:
                nonlocal x_position, y_position
                next = (x_position, y_position)
                y_position = y_position - 400
                return next

            blend_helper = usage_lookup[TEXTURE_USAGE.BLEND][0]
            blend_input, blend_frame = blend_helper.input, blend_helper.frame_node
            blend_frame.location = (-1000, 100)

            # TODO: specular for blend
            # TODO: specular for standard mats

            comp_blend = create_group_node(result, 'Composite Blend')
            comp_blend.location = (-400, 200)

            result.node_tree.links.new(comp_blend.inputs['Mask RGB'], blend_helper.color_output)
            result.node_tree.links.new(comp_blend.inputs['Mask A'], blend_helper.alpha_output)

            if diffuse_images:
                result.node_tree.links.new(bsdf.inputs['Base Color'], comp_blend.outputs['Color'])
                for helper in diffuse_images:
                    input, frame = helper.input, helper.frame_node
                    frame.location = next_position()
                    if input.blend_channel in blend_input_lookup:
                        channel = blend_input_lookup[input.blend_channel]
                        result.node_tree.links.new(comp_blend.inputs[f'{channel} Color'], helper.color_output)

            if bump_images:
                result.node_tree.links.new(bsdf.inputs['Normal'], comp_blend.outputs['Normal'])
                for helper in bump_images:
                    input, frame = helper.input, helper.frame_node
                    frame.location = next_position()
                    if input.blend_channel in blend_input_lookup:
                        channel = blend_input_lookup[input.blend_channel]
                        result.node_tree.links.new(comp_blend.inputs[f'{channel} Normal'], helper.color_output)
        else:
            # should only ever be max 1 of each input type
            if diffuse_images:
                helper = diffuse_images[0]
                helper.frame_node.location = (-700, 300)
                result.node_tree.links.new(bsdf.inputs['Base Color'], helper.color_output)
            if bump_images:
                helper = bump_images[0]
                helper.frame_node.location = (-600, -100)
                normal_node = create_group_node(result, 'DX Normal Map')
                normal_node.location = (-250, -150)
                result.node_tree.links.new(normal_node.inputs['Color'], helper.color_output)
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])

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

            self._image_lookup[index] = img

        return self._image_lookup[index]
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

TextureFrame = Tuple[bpy.types.NodeFrame, bpy.types.ShaderNodeTexImage]

def _create_uvscale_node(material: bpy.types.Material, input: TextureMapping, image_node: bpy.types.Node) -> bpy.types.Node:
    if input.tiling == (1, 1):
        return None

    scale_node = create_group_node(material, 'UV Scale')
    scale_node.inputs['X'].default_value = input.tiling[0]
    scale_node.inputs['Y'].default_value = input.tiling[1]
    material.node_tree.links.new(image_node.inputs['Vector'], scale_node.outputs['Vector'])
    return scale_node

def _create_texture_frame(material: bpy.types.Material, image_node: bpy.types.ShaderNode, scale_node: bpy.types.ShaderNode) -> bpy.types.NodeFrame:
    frame = material.node_tree.nodes.new('NodeFrame')
    image_node.parent = frame
    if scale_node:
        scale_node.parent = frame
        scale_node.location = (-200, -100)
    return frame


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

        bsdf = result.node_tree.nodes["Principled BSDF"]

        factory_lookup = {
            TEXTURE_USAGE.BLEND: self._create_blend,
            TEXTURE_USAGE.DIFFUSE: self._create_diffuse,
            TEXTURE_USAGE.NORMAL: self._create_bump
        }

        usage_lookup: Dict[str, List[TextureFrame]] = {
            TEXTURE_USAGE.BLEND: [],
            TEXTURE_USAGE.DIFFUSE: [],
            TEXTURE_USAGE.NORMAL: []
        }

        for input in mat.texture_mappings:
            if not input.texture_usage in factory_lookup:
                continue

            usage_lookup[input.texture_usage].append(factory_lookup[input.texture_usage](result, input))

        diffuse_images = usage_lookup[TEXTURE_USAGE.DIFFUSE]
        bump_images = usage_lookup[TEXTURE_USAGE.NORMAL]

        if usage_lookup[TEXTURE_USAGE.BLEND]:
            x_position = -1400
            y_position = 1400

            def next_position() -> Tuple[float, float]:
                nonlocal x_position, y_position
                next = (x_position, y_position)
                y_position = y_position - 400
                return next

            blend_frame, blend_image = usage_lookup[TEXTURE_USAGE.BLEND][0]
            blend_frame.location = (-1000, 100)

            # TODO: assign blend inputs based on specific blend channel rather than index
            # TODO: specular for blend
            # TODO: specular for standard mats

            if diffuse_images:
                diffuse_blend = create_group_node(result, 'Blend Mask')
                diffuse_blend.location = (-400, 600)
                result.node_tree.links.new(diffuse_blend.inputs['Mask RGB'], blend_image.outputs['Color'])

                for (frame, diffuse_image), blend_input in zip(diffuse_images, ['R', 'G', 'B', 'A']):
                    frame.location = next_position()
                    result.node_tree.links.new(diffuse_blend.inputs[blend_input], diffuse_image.outputs['Color'])

                result.node_tree.links.new(bsdf.inputs['Base Color'], diffuse_blend.outputs['Color'])

            if bump_images:
                bump_blend = create_group_node(result, 'Blend Mask')
                bump_blend.location = (-400, -400)
                result.node_tree.links.new(bump_blend.inputs['Mask RGB'], blend_image.outputs['Color'])

                for (frame, bump_image), blend_input in zip(bump_images, ['R', 'G', 'B', 'A']):
                    frame.location = next_position()
                    result.node_tree.links.new(bump_blend.inputs[blend_input], bump_image.outputs['Color'])

                normal_node = create_group_node(result, 'DX Normal Map')
                normal_node.location = (-200, -400)
                result.node_tree.links.new(normal_node.inputs['Color'], bump_blend.outputs['Color'])
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])
        else:
            # should only ever be max 1 of each input type
            if diffuse_images:
                frame, diffuse_image = diffuse_images[0]
                frame.location = (-400, 300)
                result.node_tree.links.new(bsdf.inputs['Base Color'], diffuse_image.outputs['Color'])
            if bump_images:
                frame, bump_image = bump_images[0]
                frame.location = (-500, -100)
                normal_node = create_group_node(result, 'DX Normal Map')
                normal_node.location = (-200, -150)
                result.node_tree.links.new(normal_node.inputs['Color'], bump_image.outputs['Color'])
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])

        return result

    def _create_blend(self, material: bpy.types.Material, input: TextureMapping) -> TextureFrame:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'CHANNEL_PACKED'

        scale_node = _create_uvscale_node(material, input, image_node)

        frame = _create_texture_frame(material, image_node, scale_node)
        frame.label = input.texture_usage

        return (frame, image_node)

    def _create_diffuse(self, material: bpy.types.Material, input: TextureMapping) -> TextureFrame:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'CHANNEL_PACKED'

        scale_node = _create_uvscale_node(material, input, image_node)

        frame = _create_texture_frame(material, image_node, scale_node)
        frame.label = input.texture_usage

        return (frame, image_node)

    def _create_bump(self, material: bpy.types.Material, input: TextureMapping) -> TextureFrame:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'NONE'
        image_node.image.colorspace_settings.name = 'Non-Color'

        scale_node = _create_uvscale_node(material, input, image_node)

        frame = _create_texture_frame(material, image_node, scale_node)
        frame.label = input.texture_usage

        return (frame, image_node)

    def _create_texture(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.ShaderNodeTexImage:
        scene, OPTIONS = self._scene, self._options

        # ensure each image is only loaded once, regardless of how many materials it gets used in
        if input.texture_index not in self._image_lookup:
            src = scene.texture_pool[input.texture_index]
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
                img = bpy.data.images.load(src_path)

            self._image_lookup[input.texture_index] = img

        result = material.node_tree.nodes.new('ShaderNodeTexImage')
        result.image = self._image_lookup[input.texture_index]
        return result
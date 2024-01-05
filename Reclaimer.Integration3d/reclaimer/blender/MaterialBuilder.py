import bpy

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


class MaterialBuilder:
    _scene: Scene
    _options: ImportOptions

    def __init__(self, scene: Scene, options: ImportOptions):
        self._scene = scene
        self._options = options

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

        usage_lookup = {
            TEXTURE_USAGE.BLEND: [],
            TEXTURE_USAGE.DIFFUSE: [],
            TEXTURE_USAGE.NORMAL: []
        }

        for input in mat.texture_mappings:
            if not input.texture_usage in factory_lookup:
                continue

            # TODO: node tree layout
            usage_lookup[input.texture_usage].append(factory_lookup[input.texture_usage](result, input))

        diffuse_images = usage_lookup[TEXTURE_USAGE.DIFFUSE]
        bump_images = usage_lookup[TEXTURE_USAGE.NORMAL]

        if usage_lookup[TEXTURE_USAGE.BLEND]:
            blend_image = usage_lookup[TEXTURE_USAGE.BLEND][0]

            # TODO: assign blend inputs based on specific blend channel rather than index

            if diffuse_images:
                diffuse_blend = create_group_node(result, 'Blend Mask')
                result.node_tree.links.new(diffuse_blend.inputs['Mask RGB'], blend_image.outputs['Color'])

                for diffuse_image, blend_input in zip(diffuse_images, ['R', 'G', 'B', 'A']):
                    result.node_tree.links.new(diffuse_blend.inputs[blend_input], diffuse_image.outputs['Color'])

                result.node_tree.links.new(bsdf.inputs['Base Color'], diffuse_blend.outputs['Color'])


            if bump_images:
                bump_blend = create_group_node(result, 'Blend Mask')
                result.node_tree.links.new(bump_blend.inputs['Mask RGB'], blend_image.outputs['Color'])

                for bump_image, blend_input in zip(bump_images, ['R', 'G', 'B', 'A']):
                    result.node_tree.links.new(bump_blend.inputs[blend_input], bump_image.outputs['Color'])

                normal_node = create_group_node(result, 'DX Normal Map')
                result.node_tree.links.new(normal_node.inputs['Color'], bump_blend.outputs['Color'])
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])
        else:
            if diffuse_images:
                diffuse_image = diffuse_images[0]
                result.node_tree.links.new(bsdf.inputs['Base Color'], diffuse_image.outputs['Color'])
            if bump_images:
                bump_image = bump_images[0]
                normal_node = create_group_node(result, 'DX Normal Map')
                result.node_tree.links.new(normal_node.inputs['Color'], bump_image.outputs['Color'])
                result.node_tree.links.new(bsdf.inputs['Normal'], normal_node.outputs['Normal'])

        return result

    def _create_blend(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.Node:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'CHANNEL_PACKED'

        scale_node = _create_uvscale_node(material, input, image_node)

        return image_node

    def _create_diffuse(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.Node:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'CHANNEL_PACKED'

        scale_node = _create_uvscale_node(material, input, image_node)

        return image_node

    def _create_bump(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.Node:
        image_node = self._create_texture(material, input)
        image_node.image.alpha_mode = 'NONE'
        image_node.image.colorspace_settings.name = 'Non-Color'

        scale_node = _create_uvscale_node(material, input, image_node)

        return image_node

    def _create_texture(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.Node:
        scene, OPTIONS = self._scene, self._options

        # TODO: create image list and access with texture index (to ensure no duplicate images being loaded)

        src = scene.texture_pool[input.texture_index]
        result = material.node_tree.nodes.new('ShaderNodeTexImage')

        if src.size > 0:
            print(f'loading embedded texture: {src.name} @ {src.address}')
            pixel_data = SceneReader.read_texture(scene, src)
            print(f'>>> {len(pixel_data)} bytes loaded')

            # create a new empty image and pack it with the embedded pixel data
            img = bpy.data.images.new(name=src.name, width=1, height=1)
            img.pack(data=pixel_data, data_len=src.size)
            img.source = 'FILE' # images.new() initially starts as 'GENERATED'

            result.image = img
        else:
            src_path = OPTIONS.texture_path(src)
            print(f'loading texture: {src_path}')
            result.image = bpy.data.images.load(src_path)

        return result
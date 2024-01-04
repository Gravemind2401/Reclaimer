import bpy

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

        for input in mat.texture_mappings:
            if input.texture_usage != TEXTURE_USAGE.DIFFUSE:
                continue # TODO

            texture = self._create_diffuse(result, input)
            result.node_tree.links.new(bsdf.inputs['Base Color'], texture.outputs['Color'])

            break

        return result

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

        src = scene.texture_pool[input.texture_index]
        src_path = OPTIONS.texture_path(src)

        print(f'loading texture: {src_path}')

        result = material.node_tree.nodes.new('ShaderNodeTexImage')
        result.image = bpy.data.images.load(src_path)

        return result
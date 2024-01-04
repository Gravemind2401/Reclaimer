import bpy

from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Material import *

__all__ = [
    'MaterialBuilder'
]


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

            texture = self._create_texture(result, input)
            texture.image.alpha_mode = 'CHANNEL_PACKED'
            result.node_tree.links.new(bsdf.inputs['Base Color'], texture.outputs['Color'])

            break

        return result

    def _create_texture(self, material: bpy.types.Material, input: TextureMapping) -> bpy.types.Node:
        scene, OPTIONS = self._scene, self._options

        src = scene.texture_pool[input.texture_index]
        src_path = OPTIONS.texture_path(src)

        print(f'loading texture: {src_path}')

        result = material.node_tree.nodes.new('ShaderNodeTexImage')
        result.image = bpy.data.images.load(src_path)

        if input.texture_usage == TEXTURE_USAGE.BUMP:
            result.image.alpha_mode = 'NONE'
            result.image.colorspace_settings.name = 'Non-Color'

        return result
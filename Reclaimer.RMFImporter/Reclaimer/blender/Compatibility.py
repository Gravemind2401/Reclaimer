import bpy
from typing import Dict, Sequence

__all__ = [
    'NodeSocketInterfaceCompat',
    'SPECULAR_SOCKET_COMPAT',
    'ShaderNodeCompat',
    'ShaderNodeSeparateColorCompat',
    'ShaderNodeCombineColorCompat',
    'ShaderNodeMixColorCompat'
]

NodeSocketInterfaceCompat = bpy.types.NodeSocketInterface if bpy.app.version[0] < 4 else bpy.types.NodeTreeInterfaceSocket

SPECULAR_SOCKET_COMPAT = 'Specular' if bpy.app.version[0] < 4 else 'Specular IOR Level'


class ShaderNodeCompat:
    inputs: Dict[str, bpy.types.NodeSocket]
    outputs: Dict[str, bpy.types.NodeSocket]
    node: bpy.types.ShaderNode

    @property
    def parent(self) -> bpy.types.Node:
        return self.node.parent

    @parent.setter
    def parent(self, value: bpy.types.Node):
        self.node.parent = value

    @property
    def label(self) -> str:
        return self.node.label

    @label.setter
    def label(self, value: str):
        self.node.label = value

    @property
    def location(self) -> Sequence[float]:
        return self.node.location

    @location.setter
    def location(self, value: Sequence[float]):
        self.node.location = value


class ShaderNodeSeparateColorCompat(ShaderNodeCompat):
    def __init__(self, node_tree: bpy.types.NodeTree) -> None:
        if bpy.app.version < (3,3):
            node = self.node = node_tree.nodes.new('ShaderNodeSeparateRGB')
            self.inputs = {
                'Color': node.inputs['Image']
            }
            self.outputs = {
                'R': node.outputs['R'],
                'G': node.outputs['G'],
                'B': node.outputs['B']
            }
        else:
            node = self.node = node_tree.nodes.new('ShaderNodeSeparateColor')
            node.mode = 'RGB'
            self.inputs = {
                'Color': node.inputs['Color']
            }
            self.outputs = {
                'R': node.outputs['Red'],
                'G': node.outputs['Green'],
                'B': node.outputs['Blue']
            }


class ShaderNodeCombineColorCompat(ShaderNodeCompat):
    def __init__(self, node_tree: bpy.types.NodeTree) -> None:
        if bpy.app.version < (3,3):
            node = self.node = node_tree.nodes.new('ShaderNodeCombineRGB')
            self.inputs = {
                'R': node.inputs['R'],
                'G': node.inputs['G'],
                'B': node.inputs['B']
            }
            self.outputs = {
                'Color': node.outputs['Image']
            }
        else:
            node = self.node = node_tree.nodes.new('ShaderNodeCombineColor')
            node.mode = 'RGB'
            self.inputs = {
                'R': node.inputs['Red'],
                'G': node.inputs['Green'],
                'B': node.inputs['Blue']
            }
            self.outputs = {
                'Color': node.outputs['Color']
            }


class ShaderNodeMixColorCompat(ShaderNodeCompat):

    @property
    def use_clamp(self) -> bool:
        return self.node.use_clamp if bpy.app.version < (3,5) else self.node.clamp_result

    @use_clamp.setter
    def use_clamp(self, value: bool):
        if bpy.app.version < (3,5):
            self.node.use_clamp = value
        else:
            self.node.clamp_result = value

    @property
    def blend_type(self) -> str:
        return self.node.blend_type

    @blend_type.setter
    def blend_type(self, value: str):
        self.node.blend_type = value

    def __init__(self, node_tree: bpy.types.NodeTree) -> None:
        if bpy.app.version < (3,5):
            node = self.node = node_tree.nodes.new('ShaderNodeMixRGB')
            self.inputs = {
                0: node.inputs[0],
                'Color1': node.inputs['Color1'],
                'Color2': node.inputs['Color2']
            }
            self.outputs = {
                'Color': node.outputs['Color']
            }
        else:
            node = self.node = node_tree.nodes.new('ShaderNodeMix')
            node.data_type = 'RGBA'
            # ShaderNodeMix has 3 sets of inputs and outputs with the same names
            # one for each data_type (float, vector, color)
            # inputs: Factor, Factor, A, B, A, B, A, B
            # ouputs: Result, Result, Result
            self.inputs = {
                0: node.inputs[0],
                'Color1': node.inputs[6],
                'Color2': node.inputs[7]
            }
            self.outputs = {
                'Color': node.outputs[2]
            }
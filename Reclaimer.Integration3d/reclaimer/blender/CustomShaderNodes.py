import bpy
from typing import cast

__all__ = [
    'init_custom_node_groups',
    'create_group_node'
]

def init_custom_node_groups():
    _initgroup_uvscale()
    _initgroup_dxnormal()

def create_group_node(material: bpy.types.Material, group_name: str) -> bpy.types.Node:
    group_node = material.node_tree.nodes.new('ShaderNodeGroup')
    group_node.node_tree = bpy.data.node_groups.get(group_name)
    return group_node

def _initgroup_uvscale():
    group = bpy.data.node_groups.new('UV Scale', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-400, -50)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (200, 50)

    group.inputs.new('NodeSocketFloat', 'X').default_value = 1
    group.inputs.new('NodeSocketFloat', 'Y').default_value = 1
    group.outputs.new('NodeSocketVector', 'Vector')

    tex_node = group.nodes.new('ShaderNodeTexCoord')
    tex_node.location = (-200, 300)

    combine_node = group.nodes.new('ShaderNodeCombineXYZ')
    combine_node.location = (-200, -50)

    multiply_node = group.nodes.new('ShaderNodeVectorMath')
    multiply_node.operation = 'MULTIPLY'
    multiply_node.location = (0, 50)

    group.links.new(combine_node.inputs['X'], group_input.outputs[0])
    group.links.new(combine_node.inputs['Y'], group_input.outputs[1])

    group.links.new(multiply_node.inputs[0], tex_node.outputs['UV'])
    group.links.new(multiply_node.inputs[1], combine_node.outputs['Vector'])

    group.links.new(group_output.inputs[0], multiply_node.outputs['Vector'])

def _initgroup_dxnormal():
    group = bpy.data.node_groups.new('DX Normal Map', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-600, 50)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (400, 50)

    group.inputs.new('NodeSocketColor', 'Color')
    group.outputs.new('NodeSocketVector', 'Normal')

    split_node = group.nodes.new('ShaderNodeSeparateRGB')
    split_node.location = (-400, 50)

    invert_node = group.nodes.new('ShaderNodeInvert')
    invert_node.location = (-200, 50)

    combine_node = group.nodes.new('ShaderNodeCombineRGB')
    combine_node.location = (0, 50)

    normal_node = cast(bpy.types.ShaderNodeNormalMap, group.nodes.new('ShaderNodeNormalMap'))
    normal_node.space = 'TANGENT'
    normal_node.location = (200, 50)

    group.links.new(split_node.inputs['Image'], group_input.outputs[0])

    group.links.new(invert_node.inputs['Color'], split_node.outputs['G'])

    group.links.new(combine_node.inputs['R'], split_node.outputs['R'])
    group.links.new(combine_node.inputs['G'], invert_node.outputs['Color'])
    group.links.new(combine_node.inputs['B'], split_node.outputs['B'])

    group.links.new(normal_node.inputs['Color'], combine_node.outputs['Image'])

    group.links.new(group_output.inputs[0], normal_node.outputs['Normal'])
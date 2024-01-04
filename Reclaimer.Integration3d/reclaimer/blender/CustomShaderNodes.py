import bpy
from typing import cast

__all__ = [
    'init_custom_node_groups',
    'create_group_node'
]

def init_custom_node_groups():
    _initgroup_uvscale()
    _initgroup_dxnormal()
    _initgroup_blendmask()

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

    normal_node = group.nodes.new('ShaderNodeNormalMap')
    normal_node.space = 'TANGENT'
    normal_node.location = (200, 50)

    group.links.new(split_node.inputs['Image'], group_input.outputs[0])

    group.links.new(invert_node.inputs['Color'], split_node.outputs['G'])

    group.links.new(combine_node.inputs['R'], split_node.outputs['R'])
    group.links.new(combine_node.inputs['G'], invert_node.outputs['Color'])
    group.links.new(combine_node.inputs['B'], split_node.outputs['B'])

    group.links.new(normal_node.inputs['Color'], combine_node.outputs['Image'])

    group.links.new(group_output.inputs[0], normal_node.outputs['Normal'])

def _initgroup_blendmask():
    group = bpy.data.node_groups.new('Blend Mask', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-400, 100)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (800, 50)

    group.inputs.new('NodeSocketColor', 'Mask RGB')
    group.inputs.new('NodeSocketColor', 'Mask A')
    group.inputs.new('NodeSocketColor', 'R')
    group.inputs.new('NodeSocketColor', 'G')
    group.inputs.new('NodeSocketColor', 'B')
    group.inputs.new('NodeSocketColor', 'A')
    group.outputs.new('NodeSocketColor', 'Color')

    def create_mix_node(mode: str, input1: bpy.types.NodeSocket, input2: bpy.types.NodeSocket) -> bpy.types.Node:
        mix_node = cast(bpy.types.ShaderNodeMixRGB, group.nodes.new('ShaderNodeMixRGB'))
        mix_node.inputs[0].default_value = 1
        mix_node.use_clamp = True
        mix_node.blend_type = mode
        group.links.new(mix_node.inputs['Color1'], input1)
        group.links.new(mix_node.inputs['Color2'], input2)

        return mix_node

    split_node = group.nodes.new('ShaderNodeSeparateRGB')
    split_node.location = (-200, 300)
    group.links.new(split_node.inputs['Image'], group_input.outputs['Mask RGB'])

    multiply_r = create_mix_node('MULTIPLY', split_node.outputs['R'], group_input.outputs['R'])
    multiply_r.label = 'Multiply R'
    multiply_r.location = (0, 400)

    multiply_g = create_mix_node('MULTIPLY', split_node.outputs['G'], group_input.outputs['G'])
    multiply_g.label = 'Multiply G'
    multiply_g.location = (0, 200)

    multiply_b = create_mix_node('MULTIPLY', split_node.outputs['B'], group_input.outputs['B'])
    multiply_b.label = 'Multiply B'
    multiply_b.location = (0, 0)

    multiply_a = create_mix_node('MULTIPLY', group_input.outputs['Mask A'], group_input.outputs['A'])
    multiply_a.label = 'Multiply A'
    multiply_a.location = (0, -200)

    add_rg = create_mix_node('ADD', multiply_r.outputs['Color'], multiply_g.outputs['Color'])
    add_rg.label = 'Add R+G'
    add_rg.location = (200, 300)

    add_rgb = create_mix_node('ADD', add_rg.outputs['Color'], multiply_b.outputs['Color'])
    add_rgb.label = 'Add RG+B'
    add_rgb.location = (400, 200)

    add_rgba = create_mix_node('ADD', add_rgb.outputs['Color'], multiply_a.outputs['Color'])
    add_rgba.label = 'Add RGB+A'
    add_rgba.location = (600, 100)

    group.links.new(group_output.inputs['Color'], add_rgba.outputs['Color'])
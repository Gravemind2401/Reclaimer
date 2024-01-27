import bpy
from typing import cast, Union, Iterable

__all__ = [
    'init_custom_node_groups',
    'create_group_node'
]

NodeSocketInterfaceCompat = bpy.types.NodeSocketInterface if bpy.app.version[0] < 4 else bpy.types.NodeTreeInterfaceSocket


def init_custom_node_groups():
    _initgroup_uvscale()
    _initgroup_dxnormal()
    _initgroup_blendmask()
    _initgroup_compositeblendmask()

def create_group_node(parent: Union[bpy.types.Material, bpy.types.NodeTree], group_name: str) -> bpy.types.Node:
    node_tree = parent
    if type(parent) == bpy.types.Material:
        node_tree = parent.node_tree
    group_node = node_tree.nodes.new('ShaderNodeGroup')
    group_node.node_tree = bpy.data.node_groups.get(group_name)
    return group_node

def _create_input_socket(node_tree: bpy.types.NodeTree, type: str, name: str) -> NodeSocketInterfaceCompat:
    if bpy.app.version[0] < 4:
        return node_tree.inputs.new(type, name)
    else:
        return node_tree.interface.new_socket(name=name, socket_type=type, in_out='INPUT')

def _create_output_socket(node_tree: bpy.types.NodeTree, type: str, name: str) -> NodeSocketInterfaceCompat:
    if bpy.app.version[0] < 4:
        return node_tree.outputs.new(type, name)
    else:
        return node_tree.interface.new_socket(name=name, socket_type=type, in_out='OUTPUT')

def _get_blender4_socket(node_tree: bpy.types.NodeTree, in_out: str, key: Union[int, str]) -> NodeSocketInterfaceCompat:
    def _enumerate_sockets() -> Iterable[NodeSocketInterfaceCompat]:
        nonlocal node_tree, in_out
        for item in node_tree.interface.items_tree:
            if item.item_type == 'SOCKET' and item.in_out == in_out:
                yield item

    sockets = _enumerate_sockets()
    if type(key) == str:
        for socket in sockets:
            if socket.name == key:
                return socket
    else:
        return list(sockets)[key]

def _get_input_socket(node: Union[bpy.types.Node, bpy.types.NodeTree], key: Union[int, str]) -> NodeSocketInterfaceCompat:
    if bpy.app.version[0] < 4 or isinstance(node, bpy.types.Node):
        return node.inputs[key]
    else:
        return _get_blender4_socket(node, 'INPUT', key)

def _get_output_socket(node: Union[bpy.types.Node, bpy.types.NodeTree], key: Union[int, str]) -> NodeSocketInterfaceCompat:
    if bpy.app.version[0] < 4 or isinstance(node, bpy.types.Node):
        return node.outputs[key]
    else:
        return _get_blender4_socket(node, 'OUTPUT', key)

def _initgroup_uvscale():
    group = bpy.data.node_groups.new('UV Scale', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-400, -50)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (200, 50)

    _create_input_socket(group, 'NodeSocketFloat', 'X').default_value = 1
    _create_input_socket(group, 'NodeSocketFloat', 'Y').default_value = 1
    _create_output_socket(group, 'NodeSocketVector', 'Vector')

    tex_node = group.nodes.new('ShaderNodeTexCoord')
    tex_node.location = (-200, 300)

    combine_node = group.nodes.new('ShaderNodeCombineXYZ')
    combine_node.location = (-200, -50)

    multiply_node = group.nodes.new('ShaderNodeVectorMath')
    multiply_node.operation = 'MULTIPLY'
    multiply_node.location = (0, 50)

    group.links.new(_get_input_socket(combine_node, 'X'), _get_output_socket(group_input, 0))
    group.links.new(_get_input_socket(combine_node, 'Y'), _get_output_socket(group_input, 1))

    group.links.new(_get_input_socket(multiply_node, 0), _get_output_socket(tex_node, 'UV'))
    group.links.new(_get_input_socket(multiply_node, 1), _get_output_socket(combine_node, 'Vector'))

    group.links.new(_get_input_socket(group_output, 0), _get_output_socket(multiply_node, 'Vector'))

def _initgroup_dxnormal():
    group = bpy.data.node_groups.new('DX Normal Map', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-600, 50)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (400, 50)

    _create_input_socket(group, 'NodeSocketColor', 'Color')
    _create_output_socket(group, 'NodeSocketVector', 'Normal')

    split_node = group.nodes.new('ShaderNodeSeparateRGB')
    split_node.location = (-400, 50)

    invert_node = group.nodes.new('ShaderNodeInvert')
    invert_node.location = (-200, 50)

    combine_node = group.nodes.new('ShaderNodeCombineRGB')
    combine_node.location = (0, 50)

    normal_node = group.nodes.new('ShaderNodeNormalMap')
    normal_node.space = 'TANGENT'
    normal_node.location = (200, 50)

    group.links.new(_get_input_socket(split_node, 'Image'), _get_output_socket(group_input, 0))

    group.links.new(_get_input_socket(invert_node, 'Color'), _get_output_socket(split_node, 'G'))

    group.links.new(_get_input_socket(combine_node, 'R'), _get_output_socket(split_node, 'R'))
    group.links.new(_get_input_socket(combine_node, 'G'), _get_output_socket(invert_node, 'Color'))
    group.links.new(_get_input_socket(combine_node, 'B'), _get_output_socket(split_node, 'B'))

    group.links.new(_get_input_socket(normal_node, 'Color'), _get_output_socket(combine_node, 'Image'))

    group.links.new(_get_input_socket(group_output, 0), _get_output_socket(normal_node, 'Normal'))

def _initgroup_blendmask():
    group = bpy.data.node_groups.new('Blend Mask', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-400, 100)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (800, 50)

    _create_input_socket(group, 'NodeSocketColor', 'Mask RGB')
    _create_input_socket(group, 'NodeSocketColor', 'Mask A')
    _create_input_socket(group, 'NodeSocketColor', 'R')
    _create_input_socket(group, 'NodeSocketColor', 'G')
    _create_input_socket(group, 'NodeSocketColor', 'B')
    _create_input_socket(group, 'NodeSocketColor', 'A')
    _create_output_socket(group, 'NodeSocketColor', 'Color')

    def create_mix_node(mode: str, input1: bpy.types.NodeSocket, input2: bpy.types.NodeSocket) -> bpy.types.Node:
        mix_node = cast(bpy.types.ShaderNodeMixRGB, group.nodes.new('ShaderNodeMixRGB'))
        _get_input_socket(mix_node, 0).default_value = 1
        mix_node.use_clamp = True
        mix_node.blend_type = mode
        group.links.new(_get_input_socket(mix_node, 'Color1'), input1)
        group.links.new(_get_input_socket(mix_node, 'Color2'), input2)

        return mix_node

    split_node = group.nodes.new('ShaderNodeSeparateRGB')
    split_node.location = (-200, 300)
    group.links.new(_get_input_socket(split_node, 'Image'), _get_output_socket(group_input, 'Mask RGB'))

    multiply_r = create_mix_node('MULTIPLY', _get_output_socket(split_node, 'R'), _get_output_socket(group_input, 'R'))
    multiply_r.label = 'Multiply R'
    multiply_r.location = (0, 400)

    multiply_g = create_mix_node('MULTIPLY', _get_output_socket(split_node, 'G'), _get_output_socket(group_input, 'G'))
    multiply_g.label = 'Multiply G'
    multiply_g.location = (0, 200)

    multiply_b = create_mix_node('MULTIPLY', _get_output_socket(split_node, 'B'), _get_output_socket(group_input, 'B'))
    multiply_b.label = 'Multiply B'
    multiply_b.location = (0, 0)

    multiply_a = create_mix_node('MULTIPLY', _get_output_socket(group_input, 'Mask A'), _get_output_socket(group_input, 'A'))
    multiply_a.label = 'Multiply A'
    multiply_a.location = (0, -200)

    add_rg = create_mix_node('ADD', _get_output_socket(multiply_r, 'Color'), _get_output_socket(multiply_g, 'Color'))
    add_rg.label = 'Add R+G'
    add_rg.location = (200, 300)

    add_rgb = create_mix_node('ADD', _get_output_socket(add_rg, 'Color'), _get_output_socket(multiply_b, 'Color'))
    add_rgb.label = 'Add RG+B'
    add_rgb.location = (400, 200)

    add_rgba = create_mix_node('ADD', _get_output_socket(add_rgb, 'Color'), _get_output_socket(multiply_a, 'Color'))
    add_rgba.label = 'Add RGB+A'
    add_rgba.location = (600, 100)

    group.links.new(_get_input_socket(group_output, 'Color'), _get_output_socket(add_rgba, 'Color'))

def _initgroup_compositeblendmask():
    group = bpy.data.node_groups.new('Composite Blend', 'ShaderNodeTree')

    group_input = group.nodes.new('NodeGroupInput')
    group_input.location = (-400, 150)

    group_output = group.nodes.new('NodeGroupOutput')
    group_output.location = (400, 50)

    channel_list = ['R', 'G', 'B', 'A']
    socket_list = ['Color', 'Specular', 'Normal']
    input_type_list = ['Color', 'Float', 'Color']
    output_type_list = ['Color', 'Float', 'Vector']

    _create_input_socket(group, 'NodeSocketColor', 'Mask RGB')
    _create_input_socket(group, 'NodeSocketColor', 'Mask A')

    # create inputs for R Color, G Color ... R Specular, G Specular etc
    for socket, socket_type in zip(socket_list, input_type_list):
        for channel in channel_list:
            _create_input_socket(group, f'NodeSocket{socket_type}', f'{channel} {socket}')

    # set valid value range for specular inputs
    for channel in channel_list:
        input = _get_input_socket(group, f'{channel} Specular')
        input.min_value = 0.0
        input.default_value = 0.5
        input.max_value = 1.0

    # create outputs for Color, Specular etc
    for socket, socket_type in zip(socket_list, output_type_list):
        _create_output_socket(group, f'NodeSocket{socket_type}', socket)

    color_blend_node = create_group_node(group, 'Blend Mask')
    color_blend_node.label = 'Color Blend'
    color_blend_node.location = (0, 400)

    spec_blend_node = create_group_node(group, 'Blend Mask')
    spec_blend_node.label = 'Specular Blend'
    spec_blend_node.location = (0, 0)

    bump_blend_node = create_group_node(group, 'Blend Mask')
    bump_blend_node.label = 'Normal Blend'
    bump_blend_node.location = (0, -400)

    normal_node = create_group_node(group, 'DX Normal Map')
    normal_node.location = (200, -200)

    blend_node_list = [color_blend_node, spec_blend_node, bump_blend_node]

    for blend_node, socket in zip(blend_node_list, socket_list):
        group.links.new(_get_input_socket(blend_node, 'Mask RGB'), _get_output_socket(group_input, 'Mask RGB'))
        group.links.new(_get_input_socket(blend_node, 'Mask A'), _get_output_socket(group_input, 'Mask A'))
        for channel in channel_list:
            group.links.new(_get_input_socket(blend_node, channel), _get_output_socket(group_input, f'{channel} {socket}'))

    group.links.new(_get_input_socket(normal_node, 'Color'), _get_output_socket(bump_blend_node, 'Color'))

    group.links.new(_get_input_socket(group_output, 'Color'), _get_output_socket(color_blend_node, 'Color'))
    group.links.new(_get_input_socket(group_output, 'Specular'), _get_output_socket(spec_blend_node, 'Color'))
    group.links.new(_get_input_socket(group_output, 'Normal'), _get_output_socket(normal_node, 'Normal'))

from typing import List, Dict, Tuple, Union, Callable, TypeVar

from .Types import *
from .FileReader import FileReader
from .DataBlock import DataBlock
from .Scene import *
from .Model import *
from .Material import *
from .Vectors import VectorDescriptor
from .VertexBuffer import *
from .IndexBuffer import *

__all__ = [
    'SceneReader'
]

T = TypeVar('T')

__strings: List[str]
__vector_descriptors: List[VectorDescriptor]


# helper functions #

def _read_property_blocks(reader: FileReader, block: DataBlock) -> Dict[str, DataBlock]:
    ''' Reads the block headers (not body) of all remaining child blocks in the current parent block and indexes them by block code '''
    blocks = _read_remaining_blocks(reader, block)
    return { b.code:b for b in blocks }

def _read_remaining_blocks(reader: FileReader, block: DataBlock) -> List[DataBlock]:
    ''' Reads the block headers (not body) of all remaining child blocks in the current parent block '''
    blocks = []
    while reader.position < block.end_address:
        blocks.append(DataBlock(reader))
    return blocks

def _read_stringref(reader: FileReader) -> str:
    ''' Reads an Int32 and returns the corresponding global string '''
    index = reader.read_int32()
    return __strings[index] if index >= 0 else ''

def _decode_attributes(reader: FileReader, props: Dict[str, DataBlock], read_func: Callable[[], None]):
    ''' Seeks to the body of the attribute data block and calls `read_func` '''
    block = props.get('ATTR', None)
    if not block:
        return

    reader.position = block.start_address
    return read_func()

def _decode_block(reader: FileReader, block: DataBlock, read_func: Callable[[FileReader, DataBlock], T]) -> T:
    ''' Seeks to the body of the block and returns the result of `read_func` from that position '''
    reader.position = block.start_address
    return read_func(reader, block)

def _decode_list(reader: FileReader, block: DataBlock, read_func: Callable[[FileReader, DataBlock], T]) -> List[T]:
    ''' Seeks to and reads the body of each child in a list block '''
    return [_decode_block(reader, b, read_func) for b in block.child_blocks]

def _decode_data_block(reader: FileReader, block: DataBlock) -> Tuple[int, int]:
    ''' Reads the address and length of a `DATA` block (byte array) '''
    reader.position = block.start_address
    size = reader.read_int32()
    address = reader.position
    return (address, size)

def _decode_custom_properties(reader: FileReader, props: Dict[str, DataBlock], target: ICustomProperties):
    block = props.get('CUST', None)
    if not block:
        return

    reader.position = block.start_address
    property_count = reader.read_int32()

    def read_value(type: int):
        if type == 0:
            return reader.read_bool()
        elif type == 1:
            return reader.read_int32()
        elif type == 2:
            return reader.read_float()
        elif type == 3:
            return _read_stringref(reader)

    for _ in range(property_count):
        key = _read_stringref(reader)
        type = reader.read_byte()
        is_array = type & 1 > 0
        type = type >> 1
        value_count = reader.read_int32() if is_array else 1
        value = [read_value(type) for _ in range(value_count)] if is_array else read_value(type)
        target.custom_properties[key] = value


def _append_default_custom_properties(scene: Scene):
    def set_value_if_new(owner: ICustomProperties, key: str, value):
        if not key in owner.custom_properties:
            owner.custom_properties[key] = value

    def set_marker_props(markers: List[Marker], parent: Model):
        for m in markers:
            set_value_if_new(m, 'marker_name', m.name)

            if not parent:
                continue

            for i in m.instances:
                if i.region_index >= 0 and i.region_index < len(parent.regions):
                    region = parent.regions[i.region_index]
                    set_value_if_new(i, 'region_name', region.name)
                    if i.permutation_index >= 0 and i.permutation_index < len(region.permutations):
                        set_value_if_new(i, 'permutation_name', region.permutations[i.permutation_index].name)
                if i.bone_index >= 0 and i.bone_index < len(parent.bones):
                    set_value_if_new(i, 'bone_name', parent.bones[i.bone_index].name)

    # set_marker_props(scene.markers, None)

    for m in scene.model_pool:
        set_value_if_new(m, 'model_name', m.name)

        for b in m.bones:
            set_value_if_new(b, 'bone_name', b.name)

        set_marker_props(m.markers, m)

        for r in m.regions:
            set_value_if_new(r, 'region_name', r.name)

            for p in r.permutations:
                set_value_if_new(p, 'region_name', r.name)
                set_value_if_new(p, 'permutation_name', p.name)

    for m in scene.material_pool:
        set_value_if_new(m, 'material_name', m.name)

    for t in scene.texture_pool:
        set_value_if_new(t, 'relative_path', t.name)


# decode functions #

def _read_scene(reader: FileReader, block: DataBlock) -> Scene:
    global __strings, __vector_descriptors

    scene = Scene()
    scene.version = Version(reader.read_byte(), reader.read_byte(), reader.read_byte(), reader.read_byte())

    def read_attribute_data():
        scene.unit_scale = reader.read_float()
        scene.world_matrix = reader.read_matrix3x3()
        scene.name = _read_stringref(reader)

    props = _read_property_blocks(reader, block)
    __strings = _decode_block(reader, props['STRS'], _read_string_index)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, scene)

    scene.root_node = _decode_block(reader, props['NODE'], _read_node)
    scene.model_pool = _decode_list(reader, props['MODL[]'], _read_model)

    __vector_descriptors = _decode_list(reader, props['VECD[]'], _read_vector_descriptor)
    scene.vertex_buffer_pool = _decode_list(reader, props['VBUF[]'], _read_vertex_buffer)
    scene.index_buffer_pool = _decode_list(reader, props['IBUF[]'], _read_index_buffer)
    scene.material_pool = _decode_list(reader, props['MATL[]'], _read_material)
    scene.texture_pool = _decode_list(reader, props['BITM[]'], _read_texture)

    # cleanup since theyre no longer needed
    __strings = None
    __vector_descriptors = None

    _append_default_custom_properties(scene)
    return scene

def _read_string_index(reader: FileReader, block: DataBlock) -> List[str]:
    count = reader.read_int32()
    return [reader.read_string() for _ in range(count)]

def _read_node(reader: FileReader, block: DataBlock):
    node = SceneGroup()

    def read_attribute_data():
        node.name = _read_stringref(reader)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, node)

    node.child_groups = _decode_list(reader, props['NODE[]'], _read_node)
    node.child_objects = _decode_list(reader, props['OBJE[]'], _read_object)

    return node

def _read_object(reader: FileReader, block: DataBlock) -> Union[SceneObject, ModelRef]:
    if block.code == 'MOD*':
        return _decode_block(reader, block, _read_modelref)
    elif block.code == 'PLAC':
        return _decode_block(reader, block, _read_placement)

def _read_object_base_props(reader: FileReader, obj: SceneObject):
    obj.name = _read_stringref(reader)
    obj.flags = reader.read_int32()

def _read_placement(reader: FileReader, block: DataBlock) -> Placement:
    placement = Placement()

    def read_attribute_data():
        _read_object_base_props(reader, placement)
        placement.transform = reader.read_matrix3x4()

    def _read_object_block(reader, wrapper_block):
        object_block = DataBlock(reader)
        return _read_object(reader, object_block)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, placement)

    # the object may be one of multiple types, so it is always wrapped in an OBJE block allowing to be accessed by key without knowing the type
    placement.object = _decode_block(reader, props['OBJE'], _read_object_block)

    return placement


def _read_modelref(reader: FileReader, block: DataBlock) -> ModelRef:
    return ModelRef(reader.read_int32())

def _read_model(reader: FileReader, block: DataBlock) -> Model:
    model = Model()

    def read_attribute_data():
        _read_object_base_props(reader, model)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, model)

    model.regions = _decode_list(reader, props['REGN[]'], _read_region)
    model.markers = _decode_list(reader, props['MARK[]'], _read_marker)
    model.bones = _decode_list(reader, props['BONE[]'], _read_bone)
    model.meshes = _decode_list(reader, props['MESH[]'], _read_mesh)
    return model

def _read_region(reader: FileReader, block: DataBlock) -> ModelRegion:
    region = ModelRegion()

    def read_attribute_data():
        region.name = _read_stringref(reader)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, region)

    region.permutations = _decode_list(reader, props['PERM[]'], _read_permutation)
    return region

def _read_permutation(reader: FileReader, block: DataBlock) -> ModelPermutation:
    perm = ModelPermutation()

    def read_attribute_data():
        perm.name = _read_stringref(reader)
        perm.instanced = reader.read_bool()
        perm.mesh_index = reader.read_int32()
        perm.mesh_count = reader.read_int32()
        perm.transform = reader.read_matrix3x4()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, perm)

    return perm

def _read_marker(reader: FileReader, block: DataBlock) -> Marker:
    marker = Marker()

    def read_attribute_data():
        marker.name = _read_stringref(reader)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, marker)

    marker.instances = _decode_list(reader, props['MKIN[]'], _read_marker_instance)
    return marker

def _read_marker_instance(reader: FileReader, block: DataBlock) -> MarkerInstance:
    inst = MarkerInstance()

    def read_attribute_data():
        inst.region_index = reader.read_int32()
        inst.permutation_index = reader.read_int32()
        inst.bone_index = reader.read_int32()
        inst.position = reader.read_float3()
        inst.rotation = reader.read_float4()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, inst)

    return inst

def _read_bone(reader: FileReader, block: DataBlock) -> Bone:
    bone = Bone()

    def read_attribute_data():
        bone.name = _read_stringref(reader)
        bone.parent_index = reader.read_int32()
        bone.transform = reader.read_matrix4x4()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, bone)

    return bone

def _read_mesh(reader: FileReader, block: DataBlock) -> Mesh:
    mesh = Mesh()

    def read_attribute_data():
        mesh.vertex_buffer_index = reader.read_int32()
        mesh.index_buffer_index = reader.read_int32()
        mesh.bone_index = reader.read_int32()
        mesh.vertex_transform = reader.read_matrix3x4()
        mesh.texture_transform = reader.read_matrix3x4()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, mesh)

    mesh.segments = _decode_list(reader, props['MSEG[]'], _read_mesh_segment)
    return mesh

def _read_mesh_segment(reader: FileReader, block: DataBlock) -> MeshSegment:
    seg = MeshSegment()

    def read_attribute_data():
        seg.index_start = reader.read_int32()
        seg.index_length = reader.read_int32()
        seg.material_index = reader.read_int32()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, seg)

    return seg

def _read_material(reader: FileReader, block: DataBlock) -> Material:
    material = Material()

    def read_attribute_data():
        material.name = _read_stringref(reader)
        material.alpha_mode = _read_stringref(reader)

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, material)

    material.texture_mappings = _decode_list(reader, props['TMAP[]'], _read_texture_mapping)
    material.tints = _decode_list(reader, props['TINT[]'], _read_tint)
    return material

def _read_texture_mapping(reader: FileReader, block: DataBlock) -> TextureMapping:
    mapping = TextureMapping()

    def read_attribute_data():
        mapping.texture_usage = _read_stringref(reader)
        mapping.blend_channel = reader.read_int32()
        mapping.texture_index = reader.read_int32()
        mapping.channel_mask = reader.read_int32()
        mapping.tiling = reader.read_float2()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)

    return mapping

def _read_tint(reader: FileReader, block: DataBlock) -> Color:
    tint = TintColor()

    def read_attribute_data():
        tint.tint_usage = _read_stringref(reader)
        tint.blend_channel = reader.read_int32()
        tint.tint_color = reader.read_color()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)

    return tint

def _read_texture(reader: FileReader, block: DataBlock) -> Texture:
    texture = Texture()

    def read_attribute_data():
        texture.name = _read_stringref(reader)
        texture.gamma = reader.read_float()

    props = _read_property_blocks(reader, block)
    _decode_attributes(reader, props, read_attribute_data)
    _decode_custom_properties(reader, props, texture)

    if 'DATA' in props:
        texture.address, texture.size = _decode_data_block(reader, props['DATA'])

    return texture

def _read_index_buffer(reader: FileReader, block: DataBlock) -> IndexBuffer:
    layout = IndexLayout(reader.read_byte())
    width = reader.read_byte()
    count = reader.read_int32()
    data = reader.read_bytes(width * count)
    return IndexBuffer(layout, width, data)

def _read_vector_descriptor(reader: FileReader, block: DataBlock) -> VectorDescriptor:
    datatype = reader.read_byte()
    size = reader.read_byte()
    count = reader.read_int32()
    dimensions = []
    for _ in range(count):
        dimensions.append((reader.read_byte(), reader.read_byte()))
    return VectorDescriptor(datatype, size, dimensions)


def _read_vertex_buffer(reader: FileReader, block: DataBlock) -> VertexBuffer:
    buf = VertexBuffer()
    buf.count = reader.read_int32()
    channel_blocks = _read_remaining_blocks(reader, block)
    channel_buffers = {
        'POSN': [],
        'TEXC': [],
        'NORM': [],
        'TANG': [],
        'BNRM': [],
        'BLID': [],
        'BLWT': [],
        'COLR': []
    }

    for b in channel_blocks:
        reader.position = b.start_address
        descriptor_index = reader.read_int32()
        descriptor = __vector_descriptors[descriptor_index]
        data = reader.read_bytes(b.end_address - reader.position)
        channel = VectorBuffer(data, descriptor, buf.count)
        channel_buffers[b.code].append(channel)

    buf.position_channels = channel_buffers['POSN']
    buf.texcoord_channels = channel_buffers['TEXC']
    buf.normal_channels = channel_buffers['NORM']
    buf.blendindex_channels = channel_buffers['BLID']
    buf.blendweight_channels = channel_buffers['BLWT']
    buf.color_channels = channel_buffers['COLR']

    return buf


class SceneReader:
    @staticmethod
    def open_scene(fileName: str) -> Scene:
        reader = FileReader(fileName)
        rootBlock = DataBlock(reader)

        if rootBlock.code != 'RMF!' or rootBlock.is_list or reader.position != rootBlock.end_address:
            raise Exception('Not a valid RMF file')

        scene = _decode_block(reader, rootBlock, _read_scene)
        reader.close()

        scene._source_file = fileName
        return scene

    @staticmethod
    def read_texture(scene: Scene, texture: Texture) -> Union[bytes, None]:
        if texture.size == 0:
            return None

        reader = FileReader(scene._source_file)
        reader.seek(texture.address, 0)
        result = reader.read_bytes(texture.size)
        reader.close()

        return result
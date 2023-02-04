from typing import List, Dict, Callable, TypeVar

from .Types import *
from .FileReader import FileReader
from .DataBlock import DataBlock
from .Scene import *
from .Model import *
from .Material import *

__all__ = [
    'SceneReader'
]

T = TypeVar('T')


# helper functions #

def _read_property_blocks(reader: FileReader, block: DataBlock) -> Dict[str, DataBlock]:
    blocks = _read_remaining_blocks(reader, block)
    return { b.code:b for b in blocks }

def _read_remaining_blocks(reader: FileReader, block: DataBlock) -> List[DataBlock]:
    blocks = []
    while reader.position < block.end_address:
        blocks.append(DataBlock(reader))
    return blocks

def _read_block_list(reader: FileReader, count: int) -> List[DataBlock]:
    return [DataBlock(reader) for _ in range(count)]

def _decode_block(reader: FileReader, block: DataBlock, read_func: Callable[[FileReader, DataBlock], T]) -> T:
    reader.position = block.start_address
    return read_func(reader, block)

def _decode_list(reader: FileReader, block: DataBlock, read_func: Callable[[FileReader, DataBlock], T]) -> List[T]:
    return [_decode_block(reader, b, read_func) for b in block.child_blocks]


# decode functions #

def _read_scene(reader: FileReader, block: DataBlock) -> Scene:
    scene = Scene()
    scene.version = Version(reader.read_byte(), reader.read_byte(), reader.read_byte(), reader.read_byte())
    scene.unit_scale = reader.read_float()
    scene.world_matrix = reader.read_matrix3x3()
    scene.name = reader.read_string()
    props = _read_property_blocks(reader, block)
    scene.root_node = _decode_block(reader, props['NODE'], _read_node)
    scene.model_pool = _decode_list(reader, props['MODL[]'], _read_model)
    vertexBufferPool = props['VBUF[]']
    indexBufferPool = props['IBUF[]']
    scene.material_pool = _decode_list(reader, props['MATL[]'], _read_material)
    scene.texture_pool = _decode_list(reader, props['BITM[]'], _read_texture)

    return scene

def _read_node(reader: FileReader, block: DataBlock):
    node = SceneGroup()
    node.name = reader.read_string()
    node.child_groups = []
    node.child_objects = []
    child_blocks = _read_block_list(reader, reader.read_int32())
    for b in child_blocks:
        if b.code == 'NODE':
            node.child_groups.append(_decode_block(reader, b, _read_node))
        elif b.code == 'MOD*':
            node.child_objects.append(_decode_block(reader, b, _read_modelref))
    return node

def _read_modelref(reader: FileReader, block: DataBlock) -> ModelRef:
    return ModelRef(reader.read_int32())

def _read_model(reader: FileReader, block: DataBlock) -> Model:
    model = Model()
    model.name = reader.read_string()
    model.flags = reader.read_int32()
    props = _read_property_blocks(reader, block)
    model.regions = _decode_list(reader, props['REGN[]'], _read_region)
    model.markers = _decode_list(reader, props['MARK[]'], _read_marker)
    model.bones = _decode_list(reader, props['BONE[]'], _read_bone)
    model.meshes = _decode_list(reader, props['MESH[]'], _read_mesh)
    return model

def _read_region(reader: FileReader, block: DataBlock) -> ModelRegion:
    region = ModelRegion()
    region.name = reader.read_string()
    props = _read_property_blocks(reader, block)
    region.permutations = _decode_list(reader, props['PERM[]'], _read_permutation)
    return region

def _read_permutation(reader: FileReader, block: DataBlock) -> ModelPermutation:
    perm = ModelPermutation()
    perm.name = reader.read_string()
    perm.instanced = reader.read_bool()
    perm.mesh_index = reader.read_int32()
    perm.mesh_count = reader.read_int32()
    perm.transform = reader.read_matrix3x4()
    return perm

def _read_marker(reader: FileReader, block: DataBlock) -> Marker:
    marker = Marker()
    marker.name = reader.read_string()
    props = _read_property_blocks(reader, block)
    marker.instances = _decode_list(reader, props['MKIN[]'], _read_marker_instance)
    return marker

def _read_marker_instance(reader: FileReader, block: DataBlock) -> MarkerInstance:
    inst = MarkerInstance()
    inst.region_index = reader.read_int32()
    inst.permutation_index = reader.read_int32()
    inst.bone_index = reader.read_int32()
    inst.position = reader.read_float3()
    inst.rotation = reader.read_float4()
    return inst

def _read_bone(reader: FileReader, block: DataBlock) -> Bone:
    bone = Bone()
    bone.name = reader.read_string()
    bone.parent_index = reader.read_int32()
    bone.transform = reader.read_matrix4x4()
    return bone

def _read_mesh(reader: FileReader, block: DataBlock) -> Mesh:
    mesh = Mesh()
    mesh.vertex_buffer_index = reader.read_int32()
    mesh.index_buffer_index = reader.read_int32()
    mesh.bone_index = reader.read_int32()
    mesh.vertex_transform = reader.read_matrix3x4()
    mesh.texture_transform = reader.read_matrix3x4()
    props = _read_property_blocks(reader, block)
    mesh.segments = _decode_list(reader, props['MSEG[]'], _read_mesh_segment)
    return mesh

def _read_mesh_segment(reader: FileReader, block: DataBlock) -> MeshSegment:
    seg = MeshSegment()
    seg.index_start = reader.read_int32()
    seg.index_length = reader.read_int32()
    seg.material_index = reader.read_int32()
    return seg

def _read_material(reader: FileReader, block: DataBlock) -> Material:
    material = Material()
    material.name = reader.read_string()
    props = _read_property_blocks(reader, block)
    material.texture_mappings = _decode_list(reader, props['TMAP[]'], _read_texture_mapping)
    material.tints = _decode_list(reader, props['TINT[]'], _read_tint)
    return material

def _read_texture_mapping(reader: FileReader, block: DataBlock) -> TextureMapping:
    mapping = TextureMapping()
    mapping.texture_index = reader.read_int32()
    mapping.tiling = reader.read_float2()
    mapping.channel_mask = reader.read_int32()
    return mapping

def _read_tint(reader: FileReader, block: DataBlock) -> Color:
    return reader.read_color()

def _read_texture(reader: FileReader, block: DataBlock) -> Texture:
    texture = Texture()
    texture.name = reader.read_string()
    texture.size = reader.read_int32()
    return texture


class SceneReader:
    @staticmethod
    def open_scene(fileName: str) -> Scene:
        reader = FileReader(fileName)
        rootBlock = DataBlock(reader)

        if rootBlock.code != 'RMF!' or rootBlock.is_list or reader.position != rootBlock.end_address:
            raise Exception('Not a valid RMF file')

        scene = _decode_block(reader, rootBlock, _read_scene)
        reader.close()

        return scene
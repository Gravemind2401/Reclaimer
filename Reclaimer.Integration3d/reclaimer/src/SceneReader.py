from dataclasses import dataclass
from typing import List
from .FileReader import *
from .Scene import *
from .Model import *

__all__ = [
    'SceneReader'
]

@dataclass
class DataBlock:
    is_list: bool
    code: str
    start_address: int
    end_address: int
    count: int
    child_blocks: List['DataBlock']

    #note: start_address and size do not include block header

    def __init__(self, reader: FileReader):
        self.code = reader.read_chars(4)
        self.is_list = self.code == 'list'
        if self.is_list:
            self.code = reader.read_chars(4) + '[]'
        self.end_address = reader.read_int32()
        if self.is_list:
            self.count = reader.read_int32()
        else:
            self.count = 0
        self.start_address = reader.position

        if self.is_list:
            self.child_blocks = []
            for _ in range(self.count):
                self.child_blocks.append(DataBlock(reader))

        reader.position = self.end_address

    def __str__(self) -> str:
        name = f'{self.code[:-1]}{self.count}]' if self.is_list else self.code
        return f'<<{name}>> @{self.start_address:08X}+{self.size:08X}'

    @property
    def size(self) -> int:
        return self.end_address - self.start_address


class SceneReader:
    _scene: Scene
    _reader: FileReader
    
    def read_scene(self, fileName: str) -> Scene:
        self._scene = Scene()
        self._reader = FileReader(fileName)

        rootBlock = DataBlock(self._reader)
        if rootBlock.code != 'RMF!' or rootBlock.is_list or self._reader.position != rootBlock.end_address:
            raise Exception('Not a valid RMF file')

        self._read_scene(rootBlock)

        self._reader.close()

    #helper functions

    def _read_property_blocks(self, end_address: int) -> dict[str, DataBlock]:
        values = self._read_remaining_blocks(end_address)
        return { block.code:block for block in values }

    def _read_remaining_blocks(self, end_address: int) -> List[DataBlock]:
        values = []
        while self._reader.position < end_address:
            values.append(DataBlock(self._reader))
        return values

    def _read_block_list(self, count: int) -> List[DataBlock]:
        values = []
        for _ in range(count):
            values.append(DataBlock(self._reader))
        return values

    #block parse functions

    def _read_scene(self, block: DataBlock):
        self._reader.position = block.start_address
        self._scene.version = Version(self._reader.read_byte(), self._reader.read_byte(), self._reader.read_byte(), self._reader.read_byte())
        self._scene.unit_scale = self._reader.read_float()
        self._reader.position += 4 * 3 * 3 #TODO: world matrix here
        self._scene.name = self._reader.read_string()

        props = self._read_property_blocks(block.end_address)

        self._scene.model_pool = [self._read_model(b) for b in props['MODL[]'].child_blocks]
        vertexBufferPool = props['VBUF[]']
        indexBufferPool = props['IBUF[]']

    def _read_node(self, block: DataBlock):
        self._reader.position = block.start_address
        name = self._reader.read_string()
        childCount = self._reader.read_int32()
        childBlocks = self._read_block_list(childCount)

    def _read_model(self, block: DataBlock) -> Model:
        model = Model()
        self._reader.position = block.start_address
        model.name = self._reader.read_string()
        model.flags = self._reader.read_int32()
        props = self._read_property_blocks(block.end_address)
        model.regions = [self._read_region(b) for b in props['REGN[]'].child_blocks]
        model.markers = [self._read_marker(b) for b in props['MARK[]'].child_blocks]
        model.bones = [self._read_bone(b) for b in props['BONE[]'].child_blocks]
        model.meshes = [self._read_mesh(b) for b in props['MESH[]'].child_blocks]
        return model

    def _read_region(self, block: DataBlock) -> List[ModelRegion]:
        region = ModelRegion()
        self._reader.position = block.start_address
        region.name = self._reader.read_string()
        region.permutations = []
        props = self._read_property_blocks(block.end_address)
        for perm_block in props['PERM[]'].child_blocks:
            perm = ModelPermutation()
            self._reader.position = perm_block.start_address
            perm.name = self._reader.read_string()
            perm.instanced = self._reader.read_bool()
            perm.mesh_index = self._reader.read_int32()
            perm.mesh_count = self._reader.read_int32()
            #TODO: read transform here
            region.permutations.append(perm)

        return region

    def _read_marker(self, block: DataBlock) -> List[Marker]:
        marker = Marker()
        self._reader.position = block.start_address
        marker.name = self._reader.read_string()
        marker.instances = []
        props = self._read_property_blocks(block.end_address)
        for inst_block in props['MKIN[]'].child_blocks:
            inst = MarkerInstance()
            self._reader.position = inst_block.start_address
            inst.region_index = self._reader.read_int32()
            inst.permutation_index = self._reader.read_int32()
            inst.bone_index = self._reader.read_int32()
            self._reader.position += 4 * 3 #TODO: position here
            self._reader.position += 4 * 4 #TODO: rotation here
            marker.instances.append(inst)
        return marker

    def _read_bone(self, block: DataBlock) -> List[Bone]:
        bone = Bone()
        self._reader.position = block.start_address
        bone.name = self._reader.read_string()
        bone.parent_index = self._reader.read_int32()
        #TODO: read transform here
        return bone

    def _read_mesh(self, block: DataBlock) -> List[Mesh]:
        mesh = Mesh()
        return mesh
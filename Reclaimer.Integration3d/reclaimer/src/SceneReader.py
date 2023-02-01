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
            self.child_blocks = list[DataBlock]()
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
        self._reader.position += 4 * 3 * 3 #world matrix here
        self._scene.name = self._reader.read_string()

        props = self._read_property_blocks(block.end_address)
        modelPool = props['MODL[]']
        vertexBufferPool = props['VBUF[]']
        indexBufferPool = props['IBUF[]']

    def _read_node(self, block: DataBlock):
        self._reader.position = block.start_address
        name = self._reader.read_string()
        childCount = self._reader.read_int32()
        childBlocks = self._read_block_list(childCount)

    def _read_model(self, block: DataBlock):
        self._reader.position = block.start_address
        name = self._reader.read_string()
        flags = self._reader.read_int32()
        props = self._read_property_blocks(block.end_address)
        regions = props['REGN[]']
        markers = props['MARK[]']
        bones = props['BONE[]']
        meshPool = props['MESH[]']
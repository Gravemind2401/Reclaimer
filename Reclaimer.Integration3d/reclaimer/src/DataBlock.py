from dataclasses import dataclass
from typing import List

from .FileReader import FileReader

__all__ = [
    'DataBlock'
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


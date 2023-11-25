from typing import List, Iterator, Callable
from collections.abc import Sequence

from .vectors.VectorDescriptor import VectorDescriptor

__all__ = [
    'VertexBuffer',
    'VectorBuffer'
]


class VertexBuffer:
    count: int
    position_channels: List['VectorBuffer']
    texcoord_channels: List['VectorBuffer']
    normal_channels: List['VectorBuffer']
    blendindex_channels: List['VectorBuffer']
    blendweight_channels: List['VectorBuffer']


class VectorBuffer(Sequence):
    _binary: bytes
    _count: int
    _decode: Callable[[bytes, int], Iterator[float]]
    _descriptor: VectorDescriptor

    def __init__(self, descriptor: VectorDescriptor, count: int, data: bytes):
        self._descriptor = descriptor
        self._count = count
        self._binary = data

    def __getitem__(self, i: int) -> Iterator[float]:
        if i < 0 or i >= self._count:
            raise IndexError('Index out of range')
        return self._descriptor.decode(self._binary, i)

    def __len__(self) -> int:
        return self._count
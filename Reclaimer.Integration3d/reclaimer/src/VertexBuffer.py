import struct
from typing import List, Tuple, Iterable, Iterator, Callable
from collections.abc import Sequence

from .vectors.BitConfig import BitConfig
from .vectors.PackedVector import *
from .vectors.NormalisedVector import *

__all__ = [
    'VertexBuffer',
    'VectorBuffer'
]

_decode_functions = {
    'VEC2': (lambda b, i: _get_real(b, i, 2)),
    'VEC3': (lambda b, i: _get_real(b, i, 3)),
    'VEC4': (lambda b, i: _get_real(b, i, 4)),
    'SDC4': (lambda b, i: _get_packed(b, i, DecN4)),
    'SDH3': (lambda b, i: _get_packed(b, i, DHenN3)),
    'SHD3': (lambda b, i: _get_packed(b, i, HenDN3)),
    'UDC4': (lambda b, i: _get_packed(b, i, UDecN4)),
    'UDH3': (lambda b, i: _get_packed(b, i, UDHenN3)),
    'UHD3': (lambda b, i: _get_packed(b, i, UHenDN3)),
    'S162': (lambda b, i: _get_normalised(b, i, 'H', 2, 2, Int16N)),
    'S164': (lambda b, i: _get_normalised(b, i, 'H', 2, 4, Int16N)),
    'U162': (lambda b, i: _get_normalised(b, i, 'H', 2, 2, UInt16N)),
    'U164': (lambda b, i: _get_normalised(b, i, 'H', 2, 4, UInt16N)),
    'SBN2': (lambda b, i: _get_normalised(b, i, 'B', 1, 2, ByteN)),
    'SBN4': (lambda b, i: _get_normalised(b, i, 'B', 1, 4, ByteN)),
    'UBN2': (lambda b, i: _get_normalised(b, i, 'B', 1, 2, UByteN)),
    'UBN4': (lambda b, i: _get_normalised(b, i, 'B', 1, 4, UByteN))
}

def _get_struct_bytes(data: bytes, offset: int, size: int) -> bytes:
    return data[offset * size:offset * size + size]

def _get_real(data: bytes, index: int, count: int) -> Tuple[float]:
    return struct.unpack('<' + 'f' * count, _get_struct_bytes(data, index, 4 * count))

def _get_packed(data: bytes, index: int, config: BitConfig) -> Iterator[float]:
    value = struct.unpack('<I', _get_struct_bytes(data, index, 4))[0]
    return PackedVector(value, config)

def _get_normalised(data: bytes, index: int, fmt: str, width: int, count: int, config: BitConfig) -> Iterator[float]:
    values = struct.unpack('<' + (fmt * count), _get_struct_bytes(data, index, width * count))
    return NormalisedVector(values, config)


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
    vector_format: str

    def __init__(self, fmt: str, count: int, data: bytes):
        self.vector_format = fmt
        self._count = count
        self._binary = data
        self._decode = _decode_functions[self.vector_format]

    def __getitem__(self, i: int) -> Iterator[float]:
        return self._decode(self._binary, i)

    def __len__(self) -> int:
        return self._count
import struct
import itertools
import operator
from typing import List, Tuple, Union, Iterator, Callable

from .BitConfig import BitConfig
from .PackedVector import *
from .NormalisedVector import *

__all__ = [
    'DescriptorFlags',
    'VectorDescriptor'
]

_integer_formats = {
    1: 'B',
    2: 'H',
    4: 'I'
}

def _get_vector_bytes(data: bytes, vector_index: int, vector_size: int) -> bytes:
    byte_index = vector_index * vector_size
    return data[byte_index:byte_index + vector_size]


class DataType:
    REAL = 0
    INTEGER = 1
    PACKED = 2

class DescriptorFlags:
    NONE = 0
    NORMALIZED = 1
    SIGN_EXTENDED = 2
    SIGN_SHIFTED = 4
    SIGN_MASK = SIGN_EXTENDED | SIGN_SHIFTED

DimensionConfig = Tuple[int, int]

class VectorDescriptor:
    _datatype: DataType
    _size: int
    _count: int
    _dimensions: List[DimensionConfig]

    _struct_format: str
    _total_bytes: int
    _bitmasks: Tuple[BitConfig]
    _decode_func: Callable[[bytes, int], Iterator[float]]

    def __init__(self, datatype: int, size: int, dimensions: List[DimensionConfig]) -> None:
        self._datatype = datatype
        self._size = size
        self._count = len(dimensions)
        self._dimensions = dimensions

        self._total_bytes = self._size * self._count

        def decode_real(data: bytes, vector_index: int) -> Iterator[float]:
            return self._unpack_struct(data, vector_index)

        def decode_integer(data: bytes, vector_index: int) -> Iterator[float]:
            values = self._unpack_struct(data, vector_index)
            return NormalisedVector(values, self._bitmasks)

        def decode_packed(data: bytes, vector_index: int) -> Iterator[float]:
            value = self._unpack_struct(data, vector_index)[0]
            return PackedVector(value, self._bitmasks)

        if (datatype == DataType.REAL):
            self._struct_format = '<' + 'f' * self._count
            self._decode_func = decode_real
            return

        if (datatype == DataType.INTEGER):
            self._struct_format = '<' + _integer_formats[self._size] * self._count
            self._bitmasks = tuple((BitConfig(0, length, flags) for flags, length in dimensions))
            self._decode_func = decode_integer
            return

        if (datatype == DataType.PACKED):
            offsets = [0, *itertools.accumulate((length for _, length in dimensions), operator.add)][:-1]

            self._total_bytes = self._size
            self._struct_format = '<' + _integer_formats[self._size]
            self._bitmasks = tuple((BitConfig(offset, length, flags) for offset, (flags, length) in zip(offsets, dimensions)))
            self._decode_func = decode_packed
            return

        raise Exception('Invalid VectorDescriptor')

    def _unpack_struct(self, data: bytes, vector_index: int) -> Union[Tuple[float, ...], Tuple[int, ...]]:
        return struct.unpack(self._struct_format, _get_vector_bytes(data, vector_index, self._total_bytes))

    def decode(self, data: bytes, vector_index: int) -> Iterator[float]:
        return self._decode_func(data, vector_index)

    def __str__(self) -> str:
        value_bits = self._size * 8
        value_count = self._count

        if self._datatype == DataType.REAL:
            return f'Float{value_bits}_{value_count}'

        sign = 'U' if (self._dimensions[0][0] & DescriptorFlags.SIGN_MASK) > 0 else ''
        norm = 'N' if (self._dimensions[0][0] & DescriptorFlags.NORMALIZED) > 0 else '_'

        if self._datatype == DataType.INTEGER:
            return f'{sign}Int{value_bits}{norm}{value_count}'

        bits_list = '_'.join(str(x[1]) for x in self._dimensions)
        return f'{sign}Pack{value_bits}{norm}_{bits_list}'
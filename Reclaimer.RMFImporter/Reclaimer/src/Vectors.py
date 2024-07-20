import struct
import itertools
import operator
from typing import List, Tuple, Union, Iterable, Callable

from .Types import IVector

__all__ = [
    'DescriptorFlags',
    'BitConfig',
    'NormalisedVector',
    'PackedVector',
    'VectorDescriptor'
]

_integer_formats = {
    1: 'B',
    2: 'H',
    4: 'I'
}

def _get_vector_bytes(data: bytes, vector_index: int, vector_size: int) -> bytes:
    ''' Gets the subset of bytes that correspond to the vector at the specified index '''
    byte_index = vector_index * vector_size
    return data[byte_index:byte_index + vector_size]

DimensionConfig = Tuple[int, int]

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


class BitConfig:
    ''' A helper class to read specific bits from an integer based on an offset and length '''

    def __init__(self, offset: int, length: int, flags: int):
        self.offset = offset
        self.length = length
        self.normalized = (flags & DescriptorFlags.NORMALIZED) > 0
        self.signMode = flags & DescriptorFlags.SIGN_MASK
        self.lengthMask = (1 << length) - 1
        self.offsetMask = ((1 << length) - 1) << offset
        if self.signMode > 0:
            self.signMask = 1 << (offset + length - 1)
            self.signExtend = 1 << length - 1;
            self.scale = float((1 << length - 1) - 1)
        else:
            self.signMask = 0
            self.signExtend = 0;
            self.scale = float((1 << length) - 1)

    def __repr__(self) -> str:
        return ('s' if self.signMode > 0 else 'u') + f'{self.length}[{self.offset}]'

    def get_value(self, bits: int) -> Union[float, int]:
        value = (bits >> self.offset) & self.lengthMask
        if self.signMode == DescriptorFlags.SIGN_SHIFTED:
            value = value - int(self.scale)
        elif self.signMode == DescriptorFlags.SIGN_EXTENDED and (bits & self.signMask) > 0:
            #python ints are arbitrary size so we need to sign extend based on the specific bit width
            value = -(value & self.signExtend) | (value & (self.signExtend - 1))
        return value / self.scale if self.normalized else value


class NormalisedVector(IVector):
    ''' A vector consisting of separate integer values that are normalised into floats '''

    _values: Tuple[int]
    _config: Tuple[BitConfig]

    def __init__(self, values: Tuple[int], config: Tuple[BitConfig]):
        self._values = values
        self._config = config

    def __getitem__(self, i: int) -> float:
        return 0 if i > len(self._values) else self._config[i].get_value(self._values[i])

    def __len__(self) -> int:
        return len(self._values)


class PackedVector(IVector):
    ''' A vector consisting of multiple values that are packed into a single integer '''

    _bits: int
    _config: Tuple[BitConfig]

    def __init__(self, value: int, config: Tuple[BitConfig]):
        self._bits = value
        self._config = config

    def __getitem__(self, i: int) -> float:
        return 0 if i > len(self._config) else self._config[i].get_value(self._bits)

    def __len__(self) -> int:
        return len(self._config)


class VectorDescriptor:
    _datatype: DataType
    _size: int
    _count: int
    _dimensions: List[DimensionConfig]

    _struct_format: str
    _total_bytes: int
    _bitmasks: Tuple[BitConfig]
    _decode_func: Callable[[bytes, int], Iterable[float]]

    def __init__(self, datatype: int, size: int, dimensions: List[DimensionConfig]) -> None:
        self._datatype = datatype
        self._size = size
        self._count = len(dimensions)
        self._dimensions = dimensions

        self._total_bytes = self._size * self._count

        # select a decode function depending on the data type

        def decode_real(data: bytes, vector_index: int) -> Iterable[float]:
            return self._unpack_struct(data, vector_index)

        def decode_integer(data: bytes, vector_index: int) -> Iterable[float]:
            values = self._unpack_struct(data, vector_index)
            return NormalisedVector(values, self._bitmasks)

        def decode_packed(data: bytes, vector_index: int) -> Iterable[float]:
            value = self._unpack_struct(data, vector_index)[0]
            return PackedVector(value, self._bitmasks)

        if (datatype == DataType.REAL):
            self._struct_format = '<' + 'f' * self._count
            self._decode_func = decode_real
            return

        if (datatype == DataType.INTEGER):
            # each dimension is stored in a separate value, so they always have an offset of 0 within that value
            self._struct_format = '<' + _integer_formats[self._size] * self._count
            self._bitmasks = tuple((BitConfig(0, length, flags) for flags, length in dimensions))
            self._decode_func = decode_integer
            return

        if (datatype == DataType.PACKED):
            # get an array of the dimension offsets based on their sizes
            # for example, sizes [10, 11, 11] would have offets [0, 10, 21]
            offsets = [0, *itertools.accumulate((length for _, length in dimensions), operator.add)][:-1]

            self._total_bytes = self._size
            self._struct_format = '<' + _integer_formats[self._size]
            self._bitmasks = tuple((BitConfig(offset, length, flags) for offset, (flags, length) in zip(offsets, dimensions)))
            self._decode_func = decode_packed
            return

        raise Exception('Invalid VectorDescriptor')

    def _unpack_struct(self, data: bytes, vector_index: int) -> Union[Tuple[float, ...], Tuple[int, ...]]:
        return struct.unpack(self._struct_format, _get_vector_bytes(data, vector_index, self._total_bytes))

    def decode(self, data: bytes, vector_index: int) -> Iterable[float]:
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
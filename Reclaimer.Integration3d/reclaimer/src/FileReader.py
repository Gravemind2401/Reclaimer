import struct
from io import BufferedReader, SEEK_SET
from enum import IntEnum #, StrEnum

from .Types import *

__all__ = [
    'SeekOrigin',
    #'ByteOrder',
    'FileReader'
]

class SeekOrigin(IntEnum):
    BEGIN = 0
    CURRENT = 1
    END = 2

#TODO: StrEnum only in 3.11+?
#class ByteOrder(StrEnum):
#    LITTLE_ENDIAN = '<'
#    BIG_ENDIAN = '>'

class FileReader:
    _stream: BufferedReader

    def __init__(self, fileName: str):
        self._stream = open(fileName, 'rb') # 'rb' = read+binary mode

    def _read(self, byteOrder: str, format: str, length: int):
        return struct.unpack(f'{byteOrder}{format}', self._stream.read(length))[0]

    def seek(self, offset: int, origin: SeekOrigin = SEEK_SET):
        self._stream.seek(offset, origin)

    def close(self):
        self._stream.close()
        self._stream = None

    @property
    def position(self) -> int:
        return self._stream.tell()

    @position.setter
    def position(self, value: int):
        self._stream.seek(value, 0)

    def read_bytes(self, length: int) -> bytes:
        return self._stream.read(length)

    def read_byte(self, byteOrder: str = '<') -> int:
        return self._read(byteOrder, 'B', 1)

    def read_bool(self) -> bool:
        return self._read('<', 'B', 1) != 0

    def read_bool32(self, byteOrder: str = '<') -> bool:
        return self.read_int32(byteOrder) != 0

    def read_uint16(self, byteOrder: str = '<') -> int:
        return self._read(byteOrder, 'H', 2)

    def read_int16(self, byteOrder: str = '<') -> int:
        return self._read(byteOrder, 'h', 2)

    def read_uint32(self, byteOrder: str = '<') -> int:
        return self._read(byteOrder, 'I', 4)

    def read_int32(self, byteOrder: str = '<') -> int:
        return self._read(byteOrder, 'i', 4)

    def read_float(self, byteOrder: str = '<') -> float:
        return self._read(byteOrder, 'f', 4)

    def read_chars(self, count: int) -> str:
        return self._stream.read(count).decode()

    def read_string(self):
        return self.read_chars(self.read_int32())

    def read_string_nullterminated(self):
        chars = []
        nextChar = self._stream.read(1).decode()
        while nextChar != chr(0):
            chars.append(nextChar)
            nextChar = self._stream.read(1).decode()
        return ''.join(chars)

    def read_color(self, byteOrder: str = '<') -> Color:
        return (self.read_byte(byteOrder), self.read_byte(byteOrder), self.read_byte(byteOrder), self.read_byte(byteOrder))

    def read_float2(self, byteOrder: str = '<') -> Float2:
        return (self.read_float(byteOrder), self.read_float(byteOrder))

    def read_float3(self, byteOrder: str = '<') -> Float3:
        return (self.read_float(byteOrder), self.read_float(byteOrder), self.read_float(byteOrder))

    def read_float4(self, byteOrder: str = '<') -> Float4:
        return (self.read_float(byteOrder), self.read_float(byteOrder), self.read_float(byteOrder), self.read_float(byteOrder))

    def read_matrix3x3(self, byteOrder: str = '<') -> Matrix4x4:
        def read_row():
            return (self.read_float(byteOrder), self.read_float(byteOrder), self.read_float(byteOrder), 0)

        return (read_row(), read_row(), read_row(), (0, 0, 0, 1))

    def read_matrix3x4(self, byteOrder: str = '<') -> Matrix4x4:
        def read_row(last: float = 0):
            return (self.read_float(byteOrder), self.read_float(byteOrder), self.read_float(byteOrder), last)

        return (read_row(), read_row(), read_row(), read_row(1))

    def read_matrix4x4(self, byteOrder: str = '<') -> Matrix4x4:
        def read_row():
            return self.read_float4(byteOrder)

        return (read_row(), read_row(), read_row(), read_row())

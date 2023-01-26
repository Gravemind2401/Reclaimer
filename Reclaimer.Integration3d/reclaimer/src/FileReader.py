import io
import struct
from enum import IntEnum, StrEnum
from sys import byteorder

class SeekOrigin(IntEnum):
    BEGIN = 0
    CURRENT = 1
    END = 2

class ByteOrder(StrEnum):
    LITTLE_ENDIAN = '<'
    BIG_ENDIAN = '>'

class FileReader:
    _stream: io.BufferedReader

    def __init__(self, fileName: str):
        self._stream = open(fileName, 'rb') # 'rb' = read+binary mode

    def _read(self, byteOrder: ByteOrder, format: str, length: int):
        return struct.unpack(f'{byteOrder}{format}', self._stream.read(length))[0]

    def seek(self, offset: int, origin: SeekOrigin = 0):
        self._stream.seek(offset, origin)

    def close(self):
        self._stream.close()
        self._stream = None

    @property
    def position(self):
        return self._stream.tell()

    @position.setter
    def position(self, value: int):
        self._stream.seek(value, 0)

    def read_bytes(self, length: int) -> bytes:
        return self._stream.read(length)

    def read_byte(self, byteOrder: ByteOrder = '<') -> int:
        return self._read(byteOrder, 'B', 1)

    def read_uint16(self, byteOrder: ByteOrder = '<') -> int:
        return self._read(byteOrder, 'H', 2)

    def read_int16(self, byteOrder: ByteOrder = '<') -> int:
        return self._read(byteOrder, 'h', 2)

    def read_uint32(self, byteOrder: ByteOrder = '<') -> int:
        return self._read(byteOrder, 'I', 4)

    def read_int32(self, byteOrder: ByteOrder = '<') -> int:
        return self._read(byteOrder, 'i', 4)

    def read_float(self, byteOrder: ByteOrder = '<') -> float:
        return self._read(byteOrder, 'f', 4)

    def read_string(self):
        chars = []
        nextChar = self._stream.read(1).decode()
        while nextChar != chr(0):
            chars.append(nextChar)
            nextChar = self._stream.read(1).decode()
        return ''.join(chars)

    #def read_vec2(self, endian = '<'):
    #    return Vector([self.read_float(endian), self.read_float(endian)])

    #def read_vec3(self, endian = '<'):
    #    return Vector([self.read_float(endian), self.read_float(endian), self.read_float(endian)])

    #def read_vec4(self, endian = '<'):
    #    return Vector([self.read_float(endian), self.read_float(endian), self.read_float(endian), self.read_float(endian)])

    #def read_quat(self, endian = '<'):
    #    vec = self.read_vec4(endian)
    #    return Quaternion([vec.w, vec.x, vec.y, vec.z])
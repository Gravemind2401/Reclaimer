from typing import Tuple

from .IVector import IVector

__all__ = [
    'BitConfig',
    'PackedVector',
    'DecN4',
    'DHenN3',
    'HenDN3',
    'UDecN4',
    'UDHenN3',
    'UHenDN3'
]


class BitConfig:
    def __init__(self, offset: int, length: int, signed: bool):
        self.offset = offset
        self.length = length
        self.signed = signed
        self.lengthMask = (1 << length) - 1
        self.offsetMask = ((1 << length) - 1) << offset
        if signed:
            self.signMask = 1 << (offset + length - 1)
            self.signExtend = 1 << length - 1;
            self.scale = float((1 << length - 1) - 1)
        else:
            self.signMask = 0
            self.signExtend = 0;
            self.scale = float((1 << length) - 1)

    def __repr__(self) -> str:
        return ('s' if self.signed else 'u') + f'{self.length}[{self.offset}]'

    def get_value(self, value: int) -> float:
        shifted = (value >> self.offset) & self.lengthMask
        if (value & self.signMask) > 0:
            #python ints are arbitrary size so we need to sign extend based on the specific bit width
            shifted = -(shifted & self.signExtend) | (shifted & (self.signExtend - 1))

        return shifted / self.scale

    @staticmethod
    def create_set(signed: bool, *precision: int) -> Tuple['BitConfig']:
        axes = []

        offset = 0
        for length in precision:
            axes.append(BitConfig(offset, length, signed))
            offset += length

        return tuple(axes)

DecN4 = BitConfig.create_set(True, 10, 10, 10, 2)
DHenN3 = BitConfig.create_set(True, 10, 11, 11)
HenDN3 = BitConfig.create_set(True, 11, 11, 10)
UDecN4 = BitConfig.create_set(False, 10, 10, 10, 2)
UDHenN3 = BitConfig.create_set(False, 10, 11, 11)
UHenDN3 = BitConfig.create_set(False, 11, 11, 10)

class PackedVector(IVector):
    def __init__(self, value: int, config: Tuple[BitConfig]):
        self._bits = value
        self._config = config

    def _width(self) -> int:
        return len(self._config)

    def _get_axis(self, axis: int) -> float:
        return 0 if len(self._config) <= axis else self._config[axis].get_value(self._bits)

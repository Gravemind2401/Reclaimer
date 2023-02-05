from typing import Tuple

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
from typing import Tuple, Union

__all__ = [
    'BitConfig'
]

class DescriptorFlags:
    NONE = 0
    NORMALIZED = 1
    SIGN_EXTENDED = 2
    SIGN_SHIFTED = 4
    SIGN_MASK = SIGN_EXTENDED | SIGN_SHIFTED

class BitConfig:
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
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
    def create_set(signed: bool, *precision: int) -> tuple['BitConfig']:
        axes = []

        offset = 0
        for length in precision:
            axes.append(BitConfig(offset, length, signed))
            offset += length

        return tuple(axes)

DecN4 = BitConfig.create_set(True, 10, 10, 10, 2)
UDecN4 = BitConfig.create_set(False, 10, 10, 10, 2)
UHenDN3 = BitConfig.create_set(False, 11, 11, 10)

class PackedVector:

    def __init__(self, value: int, config: tuple[BitConfig]):
        self._bits = value
        self._config = config

    def __repr__(self) -> str:
        return f'[{self.x:f}, {self.y:f}, {self.z:f}, {self.w:f}]'

    def _get_axis(self, axis: int) -> float:
        return 0 if len(self._config) <= axis else self._config[axis].get_value(self._bits)

    @property
    def x(self) -> float:
        return self._get_axis(0)

    @property
    def y(self) -> float:
        return self._get_axis(1)

    @property
    def z(self) -> float:
        return self._get_axis(2)

    @property
    def w(self) -> float:
        return self._get_axis(3)
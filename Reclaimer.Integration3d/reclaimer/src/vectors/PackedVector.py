from typing import Tuple

from .BitConfig import BitConfig
from .IVector import IVector

__all__ = [
    'PackedVector',
    'DecN4',
    'DHenN3',
    'HenDN3',
    'UDecN4',
    'UDHenN3',
    'UHenDN3'
]

DecN4 = BitConfig.create_set(True, 10, 10, 10, 2)
DHenN3 = BitConfig.create_set(True, 10, 11, 11)
HenDN3 = BitConfig.create_set(True, 11, 11, 10)
UDecN4 = BitConfig.create_set(False, 10, 10, 10, 2)
UDHenN3 = BitConfig.create_set(False, 10, 11, 11)
UHenDN3 = BitConfig.create_set(False, 11, 11, 10)


class PackedVector(IVector):
    _bits: int
    _config: Tuple[BitConfig]

    def __init__(self, value: int, config: Tuple[BitConfig]):
        self._bits = value
        self._config = config

    def _width(self) -> int:
        return len(self._config)

    def _get_axis(self, axis: int) -> float:
        return 0 if axis > self._width() else self._config[axis].get_value(self._bits)

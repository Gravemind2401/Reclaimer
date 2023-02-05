from typing import Tuple

from .BitConfig import BitConfig
from .IVector import IVector

__all__ = [
    'NormalisedVector',
    'Int16N',
    'UInt16N',
    'ByteN',
    'UByteN'
]

Int16N = BitConfig(0, 16, True)
UInt16N = BitConfig(0, 16, False)
ByteN = BitConfig(0, 8, True)
UByteN = BitConfig(0, 8, False)


class NormalisedVector(IVector):
    _values: Tuple[int]
    _config: BitConfig

    def __init__(self, values: Tuple[int], config: BitConfig):
        self._values = values
        self._config = config

    def __getitem__(self, i: int) -> float:
        return 0 if i > len(self._values) else self._config.get_value(self._values[i])

    def __len__(self) -> int:
        return len(self._values)

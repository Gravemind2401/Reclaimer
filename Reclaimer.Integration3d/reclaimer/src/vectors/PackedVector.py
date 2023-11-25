from typing import Tuple

from .BitConfig import BitConfig
from .IVector import IVector

__all__ = [
    'PackedVector'
]


class PackedVector(IVector):
    _bits: int
    _config: Tuple[BitConfig]

    def __init__(self, value: int, config: Tuple[BitConfig]):
        self._bits = value
        self._config = config

    def __getitem__(self, i: int) -> float:
        return 0 if i > len(self._config) else self._config[i].get_value(self._bits)

    def __len__(self) -> int:
        return len(self._config)

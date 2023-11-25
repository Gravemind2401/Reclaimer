from typing import Tuple

from .BitConfig import BitConfig
from .IVector import IVector

__all__ = [
    'NormalisedVector'
]


class NormalisedVector(IVector):
    _values: Tuple[int]
    _config: Tuple[BitConfig]

    def __init__(self, values: Tuple[int], config: Tuple[BitConfig]):
        self._values = values
        self._config = config

    def __getitem__(self, i: int) -> float:
        return 0 if i > len(self._values) else self._config[i].get_value(self._values[i])

    def __len__(self) -> int:
        return len(self._values)

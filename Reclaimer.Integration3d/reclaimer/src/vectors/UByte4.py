from typing import Tuple
from .IVector import IVector

class UByte4(IVector):
    _values: bytes

    def __init__(self, values: bytes):
        self._values = values

    def __getitem__(self, i: int) -> int:
        return 0 if i > len(self._values) else self._values[i]

    def __len__(self) -> int:
        return len(self._values)

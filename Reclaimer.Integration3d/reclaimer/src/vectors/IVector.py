from collections.abc import Sequence

class IVector(Sequence):
    def __repr__(self) -> str:
        values = ', '.join(format(f, 'f') for f in self)
        return f'[{values}]'

    def __getitem__(self, i: int) -> float:
        return 0.0

    def __len__(self) -> int:
        return 4

    @property
    def x(self) -> float:
        return self[0]

    @property
    def y(self) -> float:
        return self[1]

    @property
    def z(self) -> float:
        return self[2]

    @property
    def w(self) -> float:
        return self[3]

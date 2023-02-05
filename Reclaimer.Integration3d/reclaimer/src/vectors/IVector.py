from typing import Iterator

class IVector:
    def __repr__(self) -> str:
        values = ', '.join(format(f, 'f') for f in self)
        return f'[{values}]'

    def __iter__(self) -> Iterator[float]:
        return (self._get_axis(i) for i in range(self._width()))

    def _width(self) -> int:
        return 4

    def _get_axis(self, axis: int) -> float:
        return 0.0

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
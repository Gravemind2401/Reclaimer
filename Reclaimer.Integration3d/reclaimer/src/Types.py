from typing import Tuple, Union, Dict, List
from collections.abc import Sequence

__all__ = [
    'Triangle',
    'Color',
    'Float2',
    'Float3',
    'Float4',
    'Matrix4x4',
    'Matrix4x4_IDENTITY',
    'BasicPropertyValue',
    'IVector',
    'INamed',
    'ICustomProperties',
    'SceneObject'
]


Triangle = Tuple[int, int, int]
Color = Tuple[int, int, int, int]
Float2 = Tuple[float, float]
Float3 = Tuple[float, float, float]
Float4 = Tuple[float, float, float, float]
Matrix4x4 = Tuple[Float4, Float4, Float4, Float4]

Matrix4x4_IDENTITY = ((1., 0., 0., 0.), (0., 1., 0., 0.), (0., 0., 1., 0.), (0., 0., 0., 1.))

BasicPropertyValue = Union[int, float, str, bool]


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


class INamed:
    name: str = None

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class ICustomProperties:
    custom_properties: Dict[str, Union[BasicPropertyValue, List[BasicPropertyValue], Dict[str, BasicPropertyValue]]] = None

    def __init__(self) -> None:
        self.custom_properties = dict()


class SceneObject(INamed, ICustomProperties):
    flags: int = 0
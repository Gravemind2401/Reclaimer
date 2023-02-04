from typing import Tuple

__all__ = [
    'Float2',
    'Float3',
    'Float4',
    'Matrix3x3',
    'Matrix3x4',
    'Matrix4x4'
]

Float2 = Tuple[float, float]
Float3 = Tuple[float, float, float]
Float4 = Tuple[float, float, float, float]
Matrix3x3 = Tuple[Float3, Float3, Float3]
Matrix3x4 = Tuple[Float3, Float3, Float3, Float3]
Matrix4x4 = Tuple[Float4, Float4, Float4, Float4]
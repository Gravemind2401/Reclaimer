from dataclasses import dataclass
from typing import List

from .Types import *

__all__ = [
    'Material',
    'TextureMapping',
    'Texture'
]


class Material(INamed):
    texture_mappings: List['TextureMapping']
    tints: List[Color]


@dataclass
class TextureMapping:
    texture_index: int = -1
    tiling: Float2 = None
    channel_mask: int = 0


class Texture(INamed):
    size: int

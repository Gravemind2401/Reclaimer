from dataclasses import dataclass
from typing import List

from .Types import *

__all__ = [
    'Material',
    'TextureMapping',
    'Texture'
]

class Material:
    name: str
    texture_mappings: List['TextureMapping']
    tints: List[Color]

    def __str__(self) -> str:
        return self.name


@dataclass
class TextureMapping:
    texture_index: int = -1
    tiling: Float2 = None
    channel_mask: int = 0


@dataclass
class Texture:
    name: str = None
    size: int = 0

    def __str__(self) -> str:
        return self.name

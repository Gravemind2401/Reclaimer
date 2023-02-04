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

    def __repr__(self) -> str:
        return f'<{str(self)}>'


@dataclass
class TextureMapping:
    texture_index: int = -1
    tiling: Float2 = None
    channel_mask: int = 0


class Texture:
    name: str
    size: int

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'

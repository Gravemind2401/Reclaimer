from dataclasses import dataclass
from typing import List

from .Types import *

__all__ = [
    'TEXTURE_USAGE',
    'Material',
    'TextureMapping',
    'TintColor',
    'Texture'
]


class TEXTURE_USAGE:
    BLEND: str = 'blend'
    DIFFUSE: str = 'diffuse'
    NORMAL: str = 'bump'
    SPECULAR: str = 'specular'


class Material(INamed):
    texture_mappings: List['TextureMapping']
    tints: List[Color]


@dataclass
class TextureMapping:
    texture_usage: str = None
    blend_channel: int = 0
    texture_index: int = -1
    channel_mask: int = 0
    tiling: Float2 = None


@dataclass
class TintColor:
    tint_usage: str = None
    tint_color: Color = None

class Texture(INamed):
    address: int = 0
    size: int = 0

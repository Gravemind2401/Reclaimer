from dataclasses import dataclass
from typing import List
from enum import IntFlag

from .Types import *

__all__ = [
    'GAMMA_PRESET',
    'ALPHA_MODE',
    'TEXTURE_USAGE',
    'TINT_USAGE',
    'ChannelFlags',
    'Material',
    'TextureMapping',
    'TintColor',
    'Texture'
]


class GAMMA_PRESET:
    LINEAR = 1.0
    XRGB = 1.95
    SRGB = 2.2


class ALPHA_MODE:
    OPAQUE: str = 'opaque'
    CLIP: str = 'clip'
    ADD: str = 'additive'
    MULTIPLY: str = 'multiply'
    BLEND: str = 'blend'
    PRE_MULTIPLIED: str = 'pre_multiplied'


class TEXTURE_USAGE:
    BLEND: str = 'blend'
    DIFFUSE: str = 'diffuse'
    NORMAL: str = 'normal'
    HEIGHT: str = 'height'
    EMISSION: str = 'self_illum'
    SPECULAR: str = 'specular'
    TRANSPARENCY: str = 'transparency'
    COLOR_CHANGE: str = 'color_change'


class TINT_USAGE:
    ALBEDO = "albedo";
    EMISSION = "self_illum";
    SPECULAR = "specular";


class ChannelFlags(IntFlag):
    DEFAULT = 0

    RED = 1 << 0
    GREEN = 1 << 1
    BLUE = 1 << 2
    ALPHA = 1 << 3

    RGB = RED | GREEN | BLUE
    RGBA = RGB | ALPHA

    @staticmethod
    def get_default(usage: str) -> 'ChannelFlags':
        if usage == TEXTURE_USAGE.TRANSPARENCY:
            return ChannelFlags.ALPHA
        elif usage == TEXTURE_USAGE.BLEND:
            return ChannelFlags.RGBA
        else:
            return ChannelFlags.RGB


class Material(INamed, ICustomProperties):
    alpha_mode: str = None
    texture_mappings: List['TextureMapping']
    tints: List['TintColor']


@dataclass
class TextureMapping:
    texture_usage: str = None
    blend_channel: ChannelFlags = ChannelFlags.DEFAULT
    texture_index: int = -1
    channel_mask: ChannelFlags = ChannelFlags.DEFAULT
    tiling: Float2 = None


@dataclass
class TintColor:
    tint_usage: str = None
    blend_channel: ChannelFlags = ChannelFlags.DEFAULT
    tint_color: Color = None

class Texture(INamed, ICustomProperties):
    gamma: float = 2.2
    address: int = 0
    size: int = 0

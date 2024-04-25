from dataclasses import dataclass
from typing import List, Union, Dict, Tuple

from .Types import *
from .Model import *
from .Material import *
from .Vectors import VectorDescriptor
from .VertexBuffer import *
from .IndexBuffer import *

__all__ = [
    'Version',
    'Scene',
    'SceneGroup',
    'Placement',
    'ModelRef'
]


@dataclass
class Version:
    major: int = 0
    minor: int = 0
    build: int = 0
    revision: int = 0

    def __str__(self) -> str:
        return f'{self.major}.{self.minor}.{self.build}.{self.revision}'


class Scene(INamed):
    _source_file: str
    version: Version
    unit_scale: float
    world_matrix: Matrix4x4
    root_node: 'SceneGroup'
    markers: List[Marker]
    model_pool: List[Model]
    vertex_buffer_pool: List[VertexBuffer]
    index_buffer_pool: List[IndexBuffer]
    material_pool: List[Material]
    texture_pool: List[Texture]

    def create_texture_lookup(self, material: Material, blend_channel: ChannelFlags) -> Dict[int, Tuple[Texture, Dict[str, TextureMapping]]]:
        channel_inputs = [m for m in material.texture_mappings if m.blend_channel == blend_channel]

        #only include textures that are actually in use
        unique_indices = set(m.texture_index for m in channel_inputs)

        lookup = dict()
        for i in unique_indices:
            lookup[i] = (self.texture_pool[i], dict())

        for m in channel_inputs:
            lookup[m.texture_index][1][m.texture_usage] = m

        return lookup


class SceneGroup(INamed):
    child_groups: List['SceneGroup']
    child_objects: List[SceneObject]


class Placement(SceneObject):
    transform: Matrix4x4
    object: Union[SceneObject, 'ModelRef']


@dataclass
class ModelRef:
    model_index: int


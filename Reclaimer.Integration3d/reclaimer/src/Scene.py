from dataclasses import dataclass
from typing import List

from .Types import *
from .Model import *
from .Material import *
from .VertexBuffer import *
from .IndexBuffer import *

__all__ = [
    'Version',
    'Scene',
    'SceneGroup',
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


class SceneGroup(INamed):
    child_groups: List['SceneGroup']
    child_objects: List


@dataclass
class ModelRef:
    model_index: int


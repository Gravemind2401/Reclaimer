from dataclasses import dataclass
from typing import List

from .Types import *
from .Model import *
from .Material import *
from .VertexBuffer import *

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


class Scene:
    version: Version
    unit_scale: float
    world_matrix: Matrix3x3
    name: str
    root_node: 'SceneGroup'
    markers: List[Marker]
    model_pool: List[Model]
    vertex_buffer_pool: List[VertexBuffer]
    index_buffer_pool: List[int]
    material_pool: List[Material]
    texture_pool: List[Texture]

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class SceneGroup:
    name: str
    child_groups: List['SceneGroup']
    child_objects: List

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


@dataclass
class ModelRef:
    model_index: int


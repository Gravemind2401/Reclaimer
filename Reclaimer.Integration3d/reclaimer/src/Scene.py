from dataclasses import dataclass
from typing import List

from .Types import *
from .Model import *

__all__ = [
    'Version',
    'Scene'
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

    def __str__(self) -> str:
        return self.name

class SceneGroup:
    name: str = None
    child_groups: List['SceneGroup'] = None
    #child_objects: List[Marker] = None

    def __str__(self) -> str:
        return self.name


from dataclasses import dataclass
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

@dataclass
class Scene:
    version: Version = Version()
    unit_scale: float = 1
    name: str = None
    root_node: 'SceneGroup' = None
    markers: list[Marker] = None
    model_pool: list[Model] = None

    def __str__(self) -> str:
        return self.name

@dataclass
class SceneGroup:
    name: str = None
    child_groups: list['SceneGroup'] = None
    #child_objects: list[Marker] = None

    def __str__(self) -> str:
        return self.name


from dataclasses import dataclass
from .Model import Marker

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
    markers: list[Marker] = None


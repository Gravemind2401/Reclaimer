from dataclasses import dataclass
from .Model import Marker

@dataclass
class Scene:
    name: str = None
    markers: list[Marker] = []


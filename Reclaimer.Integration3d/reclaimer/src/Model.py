from dataclasses import dataclass

@dataclass
class Model:
    name: str = None
    flags: int = 0
    markers: list['Marker'] = []


@dataclass
class ModelRegion:
    name: str = None
    permutations: list['ModelPermutation'] = []


@dataclass
class ModelPermutation:
    name: str = None


@dataclass
class Marker:
    name: str = None
    instances: list['MarkerInstance'] = []


@dataclass
class MarkerInstance:
    position: float
    rotation: float


@dataclass
class Bone:
    name: str
    transform: float

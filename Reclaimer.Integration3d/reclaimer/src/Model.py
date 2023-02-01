from dataclasses import dataclass

__all__ = [
    'Model',
    'ModelRegion',
    'ModelPermutation',
    'Marker',
    'MarkerInstance',
    'Bone'
]

@dataclass
class Model:
    name: str = None
    flags: int = 0
    markers: list['Marker'] = None


@dataclass
class ModelRegion:
    name: str = None
    permutations: list['ModelPermutation'] = None


@dataclass
class ModelPermutation:
    name: str = None


@dataclass
class Marker:
    name: str = None
    instances: list['MarkerInstance'] = None


@dataclass
class MarkerInstance:
    position: float
    rotation: float


@dataclass
class Bone:
    name: str
    transform: float

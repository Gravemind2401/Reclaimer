from dataclasses import dataclass

__all__ = [
    'Model',
    'ModelRegion',
    'ModelPermutation',
    'Marker',
    'MarkerInstance',
    'Bone',
    'Mesh',
    'MeshSegment'
]

@dataclass
class Model:
    name: str
    flags: int
    regions: list['ModelRegion']
    markers: list['Marker']
    bones: list['Bone']
    meshes: list['Mesh']

    def __init__(self):
        pass

    def __str__(self) -> str:
        return self.name


@dataclass
class ModelRegion:
    name: str
    permutations: list['ModelPermutation']

    def __init__(self):
        pass

    def __str__(self) -> str:
        return self.name


@dataclass
class ModelPermutation:
    name: str
    instanced: bool
    mesh_index: int
    mesh_count: int
    transform: float

    def __init__(self):
        pass

    def __str__(self) -> str:
        return self.name


@dataclass
class Marker:
    name: str
    instances: list['MarkerInstance']

    def __init__(self):
        pass

    def __str__(self) -> str:
        return self.name


@dataclass
class MarkerInstance:
    position: float
    rotation: float
    region_index: int
    permutation_index: int
    bone_index: int

    def __init__(self):
        pass


@dataclass
class Bone:
    name: str
    parent_index: int
    transform: float

    def __init__(self):
        pass

    def __str__(self) -> str:
        return self.name


@dataclass
class Mesh:
    vertex_buffer_index: int
    index_buffer_index: int
    vertex_transform: float
    texture_transform: float
    segments: list['MeshSegment']

    def __init__(self):
        pass


@dataclass
class MeshSegment:
    index_start: int
    index_length: int
    material_index: int

    def __init__(self):
        pass
from dataclasses import dataclass
from typing import List

from .Types import *

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


class Model:
    name: str
    flags: int
    regions: List['ModelRegion']
    markers: List['Marker']
    bones: List['Bone']
    meshes: List['Mesh']

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class ModelRegion:
    name: str
    permutations: List['ModelPermutation']

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class ModelPermutation:
    name: str
    instanced: bool
    mesh_index: int
    mesh_count: int
    transform: Matrix3x4

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class Marker:
    name: str
    instances: List['MarkerInstance']

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


@dataclass
class MarkerInstance:
    region_index: int = -1
    permutation_index: int = -1
    bone_index: int = -1
    position: Float3 = None
    rotation: Float4 = None


class Bone:
    name: str
    parent_index: int
    transform: Matrix4x4

    def __str__(self) -> str:
        return self.name

    def __repr__(self) -> str:
        return f'<{str(self)}>'


class Mesh:
    vertex_buffer_index: int
    index_buffer_index: int
    bone_index: int # -1 if N/A
    vertex_transform: Matrix3x4
    texture_transform: Matrix3x4
    segments: List['MeshSegment']


@dataclass
class MeshSegment:
    index_start: int = 0
    index_length: int = 0
    material_index: int = -1
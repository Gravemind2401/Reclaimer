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


class Model(INamed):
    flags: int
    regions: List['ModelRegion']
    markers: List['Marker']
    bones: List['Bone']
    meshes: List['Mesh']


class ModelRegion(INamed):
    permutations: List['ModelPermutation']


class ModelPermutation(INamed):
    instanced: bool
    mesh_index: int
    mesh_count: int
    transform: Matrix4x4


class Marker(INamed):
    name: str
    instances: List['MarkerInstance']


@dataclass
class MarkerInstance:
    region_index: int = -1
    permutation_index: int = -1
    bone_index: int = -1
    position: Float3 = None
    rotation: Float4 = None


class Bone(INamed):
    parent_index: int
    transform: Matrix4x4


class Mesh:
    vertex_buffer_index: int
    index_buffer_index: int
    bone_index: int # -1 if N/A
    vertex_transform: Matrix4x4
    texture_transform: Matrix4x4
    segments: List['MeshSegment']


@dataclass
class MeshSegment:
    index_start: int = 0
    index_length: int = 0
    material_index: int = -1
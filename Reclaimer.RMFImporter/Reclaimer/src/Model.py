from dataclasses import dataclass
from typing import List, Iterator

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


class Model(SceneObject):
    original_path: str
    regions: List['ModelRegion']
    markers: List['Marker']
    bones: List['Bone']
    meshes: List['Mesh']

    def get_bone_lineage(self, bone: 'Bone') -> List['Bone']:
        lineage = [bone]
        while bone.parent_index >= 0:
            bone = self.bones[bone.parent_index]
            lineage.append(bone)
        lineage.reverse()
        return lineage
    
    def get_bone_children(self, bone: 'Bone') -> List['Bone']:
        index = self.bones.index(bone)
        return [b for b in self.bones if b.parent_index == index]


class ModelRegion(INamed, ICustomProperties):
    permutations: List['ModelPermutation']


class ModelPermutation(INamed, ICustomProperties):
    instanced: bool
    mesh_index: int
    mesh_count: int
    transform: Matrix4x4

    def get_meshes(self, model: Model) -> Iterator['Mesh']:
        for i in range(self.mesh_index, self.mesh_index + self.mesh_count):
            yield model.meshes[i]


class Marker(INamed, ICustomProperties):
    name: str
    instances: List['MarkerInstance']


class MarkerInstance(ICustomProperties):
    region_index: int = -1
    permutation_index: int = -1
    bone_index: int = -1
    position: Float3 = None
    rotation: Float4 = None


class Bone(INamed, ICustomProperties):
    parent_index: int
    transform: Matrix4x4


class Mesh(ICustomProperties):
    vertex_buffer_index: int
    index_buffer_index: int
    bone_index: int # -1 if N/A
    vertex_transform: Matrix4x4
    texture_transform: Matrix4x4
    segments: List['MeshSegment']


class MeshSegment(ICustomProperties):
    index_start: int = 0
    index_length: int = 0
    material_index: int = -1
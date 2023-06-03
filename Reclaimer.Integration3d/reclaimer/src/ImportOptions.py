from dataclasses import dataclass

from .Model import *

__all__ = [
    'ImportOptions'
]


@dataclass
class ImportOptions:
    IMPORT_BONES: bool = True
    IMPORT_MARKERS: bool = True
    IMPORT_MESHES: bool = True
    IMPORT_NORMALS: bool = True
    IMPORT_SKIN: bool = True
    IMPORT_UVW: bool = True
    IMPORT_MATERIALS: bool = True

    SPLIT_MESHES: bool = False

    BONE_SCALE: float = 1.0
    MARKER_SCALE: float = 1.0

    BONE_PREFIX: str = ''
    MARKER_PREFIX: str = '#'

    def model_name(self, model: Model):
        return f'{model.name}'

    def bone_name(self, bone: Bone):
        return f'{self.BONE_PREFIX}{bone.name}'

    def marker_name(self, marker: Marker, index: int):
        return f'{self.MARKER_PREFIX}{marker.name}'

    def region_name(self, region: ModelRegion):
        return f'{region.name}'

    def permutation_name(self, region: ModelRegion, permutation: ModelPermutation, index: int):
        return f'{region.name}:{permutation.name}'
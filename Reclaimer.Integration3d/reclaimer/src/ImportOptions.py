from pathlib import Path

from .Material import *
from .Model import *

__all__ = [
    'ImportOptions'
]


class ImportOptions:
    IMPORT_BONES: bool = True
    IMPORT_MARKERS: bool = True
    IMPORT_MESHES: bool = True
    IMPORT_MATERIALS: bool = True

    SPLIT_MESHES: bool = False
    IMPORT_NORMALS: bool = True
    IMPORT_SKIN: bool = True
    IMPORT_UVW: bool = True
    IMPORT_COLORS: bool = True

    IMPORT_CUSTOM_PROPS: bool = True

    OBJECT_SCALE: float = 1.0
    BONE_SCALE: float = 1.0
    MARKER_SCALE: float = 1.0

    BONE_PREFIX: str = ''
    MARKER_PREFIX: str = '#'

    BITMAP_ROOT: str = ''
    BITMAP_EXT: str = 'tif'

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

    def material_name(self, material: Material):
        return f'{material.name}'

    def texture_path(self, texture: Texture):
        ext = self.BITMAP_EXT if self.BITMAP_EXT else 'tif'
        path = Path(self.BITMAP_ROOT).joinpath(texture.name).with_suffix('.' + ext.lstrip('.'))
        return str(path)
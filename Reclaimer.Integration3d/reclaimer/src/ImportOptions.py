from pathlib import Path

from .Material import *
from .Scene import *
from .Model import *

__all__ = [
    'ImportOptions'
]


class ImportOptions:
    SCENE_DATA: Scene = None

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

    def permutation_name(self, region: ModelRegion, permutation: ModelPermutation, mesh_index: int, segment_index: int):
        result = f'{region.name}:{permutation.name}'
        if permutation.mesh_count > 1:
            result = result + f'[{mesh_index}]'
        if self.SPLIT_MESHES:
            result = result + f'-{segment_index:02d}'
        return result

    def material_name(self, material: Material):
        return f'{material.name}'

    def texture_path(self, texture: Texture):
        default_ext = (self.BITMAP_EXT or '').strip().lstrip('.').lower()
        default_dir = (self.BITMAP_ROOT or '').strip()
        name_only = Path(texture.name).name

        default_path = None
        if default_ext and default_dir:
            default_path = Path(default_dir).joinpath(texture.name).with_suffix(f'.{default_ext}')
            if default_path.exists():
                return str(default_path)
            else:
                print(f'WARNING: bitmap file \'{name_only}\' did not exist at {default_path}')
        else:
            print(f'attempting to locate bitmap file \'{name_only}\'...')

        def get_test_paths():
            scene = self.SCENE_DATA
            if not scene:
                return

            #attempt to make unique lists while still preserving order (set doesnt preserve order)
            dir_list = dict.fromkeys((default_dir, scene._source_dir, str(Path(scene._source_dir).joinpath(scene._source_name)))).keys()
            ext_list = dict.fromkeys((default_ext, 'tif', 'png')).keys()

            for dir in dir_list:
                for ext in ext_list:
                    if ext and dir:
                        yield Path(dir).joinpath(texture.name).with_suffix(f'.{ext}')

        for p in get_test_paths():
            if p == default_path:
                continue
            print(f'...try: {p}')
            if p.exists():
                print('found bitmap file!')
                return str(p)

        print(f'WARNING: could not find bitmap file: \'{name_only}\'')
        return str(next(get_test_paths())) # fall back to the first path checked
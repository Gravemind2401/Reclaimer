from dataclasses import dataclass

__all__ = [
    'ImportOptions'
]


@dataclass
class ImportOptions:
    IMPORT_BONES: bool = True
    IMPORT_MARKERS: bool = True
    IMPORT_MESHES: bool = True
    IMPORT_SKIN: bool = True
    IMPORT_MATERIALS: bool = True

    BONE_PREFIX: str = ''
    MARKER_PREFIX: str = '#'

from pathlib import Path
from dataclasses import dataclass
from typing import List, Dict, Tuple

from .Types import *
from .Model import *
from .Material import *
from .VertexBuffer import *
from .IndexBuffer import *

__all__ = [
    'Version',
    'Scene',
    'SceneGroup',
    'Placement'
]


@dataclass
class Version:
    major: int = 0
    minor: int = 0
    build: int = 0
    revision: int = 0

    def __str__(self) -> str:
        return f'{self.major}.{self.minor}.{self.build}.{self.revision}'


class Scene(INamed, ICustomProperties):
    _source_file: str
    _source_dir: str
    _source_name: str

    version: Version
    unit_scale: float
    world_matrix: Matrix4x4
    original_path: str
    root_node: 'SceneGroup'
    markers: List[Marker]
    model_pool: List[Model]
    vertex_buffer_pool: List[VertexBuffer]
    index_buffer_pool: List[IndexBuffer]
    material_pool: List[Material]
    texture_pool: List[Texture]

    def _set_source_file(self, path: str):
        self._source_file = path
        self._source_dir = str(Path(path).parent)
        self._source_name = Path(path).with_suffix('').stem

    def create_texture_lookup(self, material: Material, blend_channel: ChannelFlags) -> Dict[int, Tuple[Texture, Dict[str, TextureMapping]]]:
        channel_inputs = [m for m in material.texture_mappings if m.blend_channel == blend_channel]

        #only include textures that are actually in use
        unique_indices = set(m.texture_index for m in channel_inputs)

        lookup = dict()
        for i in unique_indices:
            lookup[i] = (self.texture_pool[i], dict())

        for m in channel_inputs:
            lookup[m.texture_index][1][m.texture_usage] = m

        return lookup

    def validate_mesh(self, mesh: Mesh) -> bool:
        try:
            index_buffer = self.index_buffer_pool[mesh.index_buffer_index]
            vertex_buffer = self.vertex_buffer_pool[mesh.vertex_buffer_index]

            return len(index_buffer.indices) > 0 \
                and vertex_buffer.count > 0 \
                and index_buffer.index_layout in [IndexLayout.DEFAULT, IndexLayout.TRIANGLE_LIST, IndexLayout.TRIANGLE_STRIP];
        except:
            return False


class SceneGroup(INamed, ICustomProperties):
    child_groups: List['SceneGroup']
    child_objects: List['Placement']


class Placement(SceneObject):
    transform: Matrix4x4
    object: SceneObject

import struct
from enum import IntEnum
from typing import List, Iterable, Iterator

__all__ = [
    'IndexLayout',
    'IndexBuffer'
]


class IndexLayout(IntEnum):
    DEFAULT = 0
    LINE_LIST = 1
    LINE_STRIP = 2
    TRIANGLE_LIST = 3
    TRIANGLE_PATCH = 4
    TRIANGLE_STRIP = 5
    QUAD_LIST = 6
    RECT_LIST = 7

_index_widths = ('B', 'H', None, 'I')

class IndexBuffer:
    index_layout: IndexLayout
    indices: List[int]

    def __init__(self, index_layout: IndexLayout, width: int, binary: bytes):
        if width < 0 or width > 4 or width == 3:
            raise Exception('Unsupported binary width')

        self.index_layout = index_layout
        self.indices = list(t[0] for t in struct.iter_unpack(_index_widths[width], binary))

    def __repr__(self) -> str:
        return f'<{self.__class__.name}|{IndexLayout(self.index_layout).name}|{self.indices.count}>'

    def get_triangles(self, offset: int = 0, count: int = -1) -> Iterator[int]:
        end = self.indices.count if count < 0 else offset + count
        subset = (self.indices[i] for i in range(offset, end))
        if self.index_layout == IndexLayout.TRIANGLE_LIST:
            return subset
        elif self.index_layout == IndexLayout.TRIANGLE_STRIP:
            return self._unpack_triangle_list(subset)
        else:
            raise Exception('Unsupported index layout')

    def _unpack_triangle_list(self, indices: Iterable[int]) -> Iterator[int]:
        i0, i1, i2 = 0, 0, 0
        for pos, idx in enumerate(indices):
            i0, i1, i2 = i1, i2, idx
            if pos < 2 or i0 == i1 or i0 == i2 or i1 == i2:
                continue
            yield i0
            if pos % 2 == 0:
                yield i1
                yield i2
            else:
                yield i2
                yield i1

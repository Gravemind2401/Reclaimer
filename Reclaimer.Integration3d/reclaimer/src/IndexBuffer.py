import struct
import itertools
from enum import IntEnum
from typing import List, Iterable, Iterator, overload

from .Types import Triangle
from .Model import Mesh, MeshSegment

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

_index_widths = (None, 'B', 'H', None, 'I')

class IndexBuffer:
    index_layout: IndexLayout
    indices: List[int]

    def __init__(self, index_layout: IndexLayout, width: int, data: bytes):
        if width <= 0 or width > 4 or width == 3:
            raise Exception('Unsupported binary width')

        self.index_layout = index_layout
        self.indices = list(t[0] for t in struct.iter_unpack(_index_widths[width], data))

    def __repr__(self) -> str:
        return f'<{self.__class__.name}|{IndexLayout(self.index_layout).name}|{self.indices.count}>'

    @overload
    def count_triangles(self, offset: int = 0, count: int = -1) -> int:
        ''' Gets the number of triangles in a given range of source indices '''
        ...

    @overload
    def count_triangles(self, segment: MeshSegment) -> int:
        ''' Gets the number of triangles in the index range defined in a `MeshSegment` '''
        ...

    @overload
    def count_triangles(self, mesh: Mesh) -> int:
        ''' Gets the total number of triangles across every index range defined by the `MeshSegments` of a given `Mesh` '''
        ...

    def count_triangles(self, arg1, arg2 = None) -> int:
        def get_count(offset: int, count: int) -> int:
            if self.index_layout == IndexLayout.TRIANGLE_LIST:
                return int(count / 3)
            elif self.index_layout == IndexLayout.TRIANGLE_STRIP:
                # count the number of unpacked triangles returned
                return sum(1 for _ in self.get_triangles(offset, count))
            else:
                raise Exception('Unsupported index layout')

        def from_range(offset: int, count: int) -> int:
            return get_count(offset, count)
        
        def from_segment(segment: MeshSegment) -> int:
            return from_range(segment.index_start, segment.index_length)
        
        def from_mesh(mesh: Mesh) -> int:
            return sum(from_segment(s) for s in mesh.segments)

        if isinstance(arg1, MeshSegment):
            return from_segment(arg1)
        if isinstance(arg1, Mesh):
            return from_mesh(arg1)
        return from_range(arg1, arg2)

    @overload
    def get_triangles(self, offset: int = 0, count: int = -1) -> Iterator[Triangle]:
        ''' Iterates the triangles for a given range of source indices '''
        ...

    @overload
    def get_triangles(self, segment: MeshSegment) -> Iterator[Triangle]:
        ''' Iterates the triangles for the index range defined in a `MeshSegment` '''
        ...

    @overload
    def get_triangles(self, mesh: Mesh) -> Iterator[Triangle]:
        ''' Iterates the triangles across every index range defined by the `MeshSegments` of a given `Mesh` '''
        ...

    def get_triangles(self, arg1, arg2 = None) -> Iterator[Triangle]:
        def get_indices(offset: int, count: int) -> Iterator[int]:
            end = self.indices.count if count < 0 else offset + count
            subset = (self.indices[i] for i in range(offset, end))
            if self.index_layout == IndexLayout.TRIANGLE_LIST:
                return subset
            elif self.index_layout == IndexLayout.TRIANGLE_STRIP:
                return self._unpack_triangle_list(subset)
            else:
                raise Exception('Unsupported index layout')

        def from_range(offset: int, count: int) -> Iterator[Triangle]:
            indices = get_indices(offset, count)
            return iter(lambda: tuple(itertools.islice(indices, 3)), ())
        
        def from_segment(segment: MeshSegment) -> Iterator[Triangle]:
            return from_range(segment.index_start, segment.index_length)
        
        def from_mesh(mesh: Mesh) -> Iterator[Triangle]:
            for segment in mesh.segments:
                for t in from_segment(segment):
                    yield t

        if isinstance(arg1, MeshSegment):
            return from_segment(arg1)
        if isinstance(arg1, Mesh):
            return from_mesh(arg1)
        return from_range(arg1, arg2)

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

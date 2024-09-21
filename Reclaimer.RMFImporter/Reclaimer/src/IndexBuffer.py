import struct
import itertools
from enum import IntEnum
from typing import List, Tuple, Iterable, Iterator, overload

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
        self.index_layout = index_layout

        if width is None and data is None:
            return

        if width <= 0 or width > 4 or width == 3:
            raise Exception('Unsupported binary width')

        self.indices = list(t[0] for t in struct.iter_unpack(_index_widths[width], data))

    def __repr__(self) -> str:
        return f'<{self.__class__.name}|{IndexLayout(self.index_layout).name}|{self.indices.count}>'

    def get_vertex_range(self, segment: MeshSegment) -> Tuple[int, int]:
        indices = (self.indices[i] for i in range(segment.index_start, segment.index_start + segment.index_length))
        lower, upper = -1, -1
        for i, index in enumerate(indices):
            lower, upper = (index, index) if i == 0 else (min(lower, index), max(upper, index))
        return lower, upper + 1 - lower

    def relative_slice(self, segment: MeshSegment) -> 'IndexBuffer':
        result = IndexBuffer(self.index_layout, None, None)
        result.indices = self.indices[segment.index_start:(segment.index_start + segment.index_length)]

        # offset the indices so they are relative to zero (so they correspond with the vertices in a slice of the vertex buffer)
        vertex_start = min(result.indices)
        for i, value in enumerate(result.indices):
            result.indices[i] = value - vertex_start

        return result

    @overload
    def count_triangles(self, offset: int, count: int) -> int:
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
            elif self.index_layout in [IndexLayout.DEFAULT, IndexLayout.TRIANGLE_STRIP]:
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
    def get_triangles(self, offset: int, count: int) -> Iterator[Triangle]:
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
            end = len(self.indices) if count < 0 else offset + count
            subset = (self.indices[i] for i in range(offset, end))
            if self.index_layout == IndexLayout.TRIANGLE_LIST:
                return subset
            elif self.index_layout in [IndexLayout.DEFAULT, IndexLayout.TRIANGLE_STRIP]:
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

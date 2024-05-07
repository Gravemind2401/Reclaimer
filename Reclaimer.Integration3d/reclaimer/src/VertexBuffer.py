import itertools
from typing import List, Tuple, Iterator, Iterable
from collections.abc import Sequence

from .Vectors import VectorDescriptor

__all__ = [
    'VertexBuffer',
    'VectorBuffer'
]


class VertexBuffer:
    count: int
    position_channels: List['VectorBuffer']
    texcoord_channels: List['VectorBuffer']
    normal_channels: List['VectorBuffer']
    blendindex_channels: List['VectorBuffer']
    blendweight_channels: List['VectorBuffer']
    color_channels: List['VectorBuffer']

    def enumerate_blendpairs(self) -> Iterator[Tuple[int, Iterable[float], Iterable[float]]]:
        '''
        Iterates over all corresponding tuples of (index, blendindices, blendweights).
        The returned weights will be normalised and all index/weight channels will be accounted for.
        '''

        dummy_weights = [1.0]
        rigid = len(self.blendweight_channels) == 0

        if len(self.blendindex_channels) > 1:
            # join the buffers together so we have a single list of indices and a single list of weights
            # where the number of indices/weights per vertex will be the sum across all buffers
            blend_indicies = list(list(itertools.chain(*vectors)) for vectors in zip(*self.blendindex_channels))
            if not rigid:
                blend_weights = list(list(itertools.chain(*vectors)) for vectors in zip(*self.blendweight_channels))
        else:
            blend_indicies = self.blendindex_channels[0]
            if not rigid:
                blend_weights = self.blendweight_channels[0]

        for i in range(len(self.position_channels[0])):
            indices = blend_indicies[i]
            weights = dummy_weights if rigid else blend_weights[i]

            if rigid:
                # only take the first index (dummy_weights already only has one weight)
                indices = [indices[0]]
            elif 0 in weights:
                # filter out any zero weighted indices
                indices = list(indices[i] for i, w in enumerate(weights) if w > 0)
                weights = list(weights[i] for i, w in enumerate(weights) if w > 0)

            # normalise the weights before returning them
            weight_sum = sum(weights)
            if weight_sum > 0:
                weights = list(w / weight_sum for w in weights)

            yield (i, indices, weights)

    def slice(self, offset: int, count: int) -> 'VertexBuffer':
        result = VertexBuffer()
        result.count = count
        result.position_channels = list(c.slice(offset, count) for c in self.position_channels)
        result.texcoord_channels = list(c.slice(offset, count) for c in self.texcoord_channels)
        result.normal_channels = list(c.slice(offset, count) for c in self.normal_channels)
        result.blendindex_channels = list(c.slice(offset, count) for c in self.blendindex_channels)
        result.blendweight_channels = list(c.slice(offset, count) for c in self.blendweight_channels)
        result.color_channels = list(c.slice(offset, count) for c in self.color_channels)
        return result


class VectorBuffer(Sequence):
    _binary: bytes
    _descriptor: VectorDescriptor
    _count: int
    _offset: int

    def __init__(self, data: bytes, descriptor: VectorDescriptor, count: int):
        self._binary = data
        self._descriptor = descriptor
        self._count = count
        self._offset = 0

    def __getitem__(self, i: int) -> Iterable[float]:
        if i < 0 or i >= self._count:
            raise IndexError('Index out of range')
        return self._descriptor.decode(self._binary, self._offset + i)

    def __len__(self) -> int:
        return self._count

    def slice(self, offset: int, count: int) -> 'VectorBuffer':
        result = VectorBuffer(self._binary, self._descriptor, count)
        result._offset = offset
        return result;
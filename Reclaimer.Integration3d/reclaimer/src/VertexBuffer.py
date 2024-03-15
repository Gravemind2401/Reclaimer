import itertools
from typing import List, Tuple, Iterator, Iterable, Callable
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


class VectorBuffer(Sequence):
    _binary: bytes
    _count: int
    _decode: Callable[[bytes, int], Iterable[float]]
    _descriptor: VectorDescriptor

    def __init__(self, descriptor: VectorDescriptor, count: int, data: bytes):
        self._descriptor = descriptor
        self._count = count
        self._binary = data

    def __getitem__(self, i: int) -> Iterable[float]:
        if i < 0 or i >= self._count:
            raise IndexError('Index out of range')
        return self._descriptor.decode(self._binary, i)

    def __len__(self) -> int:
        return self._count
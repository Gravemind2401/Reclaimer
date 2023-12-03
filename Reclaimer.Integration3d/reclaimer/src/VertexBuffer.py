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

    def enumerate_blendpairs(self) -> Iterator[Tuple[int, Iterable[float], Iterable[float]]]:
        '''
        Iterates over all corresponding tuples of (index, blendindices, blendweights).
        The returned weights will be normalised and all index/weight channels will be accounted for.
        '''

        # TODO: rigid_boned doesnt have weights
        # TODO: filter zero weights/indices out before returning the tuples (set 1.0 weights for rigid_boned at the same time)
        if len(self.blendindex_channels) > 1:
            # join the buffers together so we have a single list of indices and a single list of weights
            # where the number of indices/weights per vertex will be the sum across all buffers
            blend_indicies = list(list(itertools.chain(*vectors)) for vectors in zip(*self.blendindex_channels))
            blend_weights = list(list(itertools.chain(*vectors)) for vectors in zip(*self.blendweight_channels))
        else:
            blend_indicies = self.blendindex_channels[0]
            blend_weights = self.blendweight_channels[0]

        for i in range(len(self.position_channels[0])):
            # normalise the weights before returning them
            weight_sum = sum(blend_weights[i])
            normalised = list(w / weight_sum for w in blend_weights[i]) if weight_sum > 0 else blend_weights[i]

            yield (i, blend_indicies[i], normalised)


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
import bpy
from bpy.types import Context, LayerCollection, Collection
from typing import Iterator


def iterate_layer_collections(col: LayerCollection) -> Iterator[LayerCollection]:
    yield col
    for child in col.children:
        yield from iterate_layer_collections(child)

def set_active_collection(col: Collection, context: Context = None) -> bool:
    if not context:
        context = bpy.context
    for layer_col in iterate_layer_collections(context.view_layer.layer_collection):
        if layer_col.collection == col:
            context.view_layer.active_layer_collection = layer_col
            return True
    return False
from pymxs import runtime as rt

from ..src.SceneReader import SceneReader
from . import SceneBuilder

def import_rmf():
    fileName = rt.getOpenFileName(types="RMF Files (*.rmf)|*.rmf")
    if not fileName:
        return
    
    scene = SceneReader.open_scene(fileName)
    print(f'scene name: {scene.name}')
    print(f'scene scale: {scene.unit_scale}')
    for m in scene.model_pool:
        SceneBuilder.create_model(scene, m)

    rt.completeRedraw()
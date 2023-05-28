from pymxs import runtime as rt

from ..src.SceneReader import SceneReader
from . import SceneBuilder

def import_rmf():
    fileName = rt.getOpenFileName(types="RMF Files (*.rmf)|*.rmf")
    if not fileName:
        return
    
    scene = SceneReader.open_scene(fileName)
    SceneBuilder.create_scene(scene)

    rt.completeRedraw()
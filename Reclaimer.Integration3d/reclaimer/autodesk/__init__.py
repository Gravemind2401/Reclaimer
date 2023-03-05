from pymxs import runtime as rt

from .Utils import *
from ..src.SceneReader import SceneReader

def import_rmf():
    fileName = rt.getOpenFileName(types="RMF Files (*.rmf)|*.rmf")
    if not fileName:
        return
    
    scene = SceneReader.open_scene(fileName)
    print(f'scene name: {scene.name}')

    model = scene.model_pool[0]
    perm = model.regions[0].permutations[0]
    mesh = model.meshes[perm.mesh_index]

    ibuffer = scene.index_buffer_pool[mesh.index_buffer_index]
    vbuffer = scene.vertex_buffer_pool[mesh.vertex_buffer_index]

    # note 3dsMax uses 1-based indices!

    faces = [toPoint3(t) + 1 for t in ibuffer.get_triangles(mesh)]
    verts = [toPoint3(v) for v in vbuffer.position_channels[0]]

    maxMesh = rt.Mesh(vertices=verts, faces=faces)

    rt.completeRedraw()
from typing import cast
from typing import Dict, Tuple
from functools import reduce
import operator

from pymxs import runtime as rt

from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *
from .Utils import *

MX_UNITS = 100.0 # 1 max unit = 100mm?

def create_model(scene: Scene, model: Model):
    builder = ModelBuilder(scene, model)
    builder.create_bones()

class ModelBuilder:
    _scene: Scene
    _model: Model

    def __init__(self, scene: Scene, model: Model):
        self._scene = scene
        self._model = model

    def create_bones(self):
        scene, model = self._scene, self._model
        print('creating skeleton')

        UNIT_SCALE = scene.unit_scale / MX_UNITS
        PREFIX = '' # TODO
        BONE_SIZE = 0.03 * UNIT_SCALE
        TAIL_VECTOR = rt.Point3(BONE_SIZE, 0.0, 0.0)

        def makemat(mat: Matrix4x4) -> rt.Matrix3:
            m = toMatrix3(mat)
            return rt.preRotate(rt.transMatrix(m.translationPart * UNIT_SCALE), m.rotationPart)
        
        bone_transforms: list[rt.Matrix3] = []
        for b in model.bones:
            lineage = model.get_bone_lineage(b)
            lst = [makemat(x.transform) for x in reversed(lineage)]
            bone_transforms.append(reduce(operator.mul, lst))

        maxbones = []
        for i, b in enumerate(model.bones):
            maxbone = rt.BoneSys.createBone(rt.Point3(0, 0, 0), TAIL_VECTOR, rt.Point3(0, 0, 1))
            maxbone.setBoneEnable(False, 0)
            maxbone.name = b.name
            maxbone.height = maxbone.width = maxbone.length = BONE_SIZE
            maxbones.append(maxbone)

            children = model.get_bone_children(b)
            if children:
                size = max(rt.length(toPoint3(b.transform[3])) for b in children)
                maxbone.length = size * UNIT_SCALE

            maxbone.taper = 70 if children else 50
            maxbone.transform = bone_transforms[i]

            if b.parent_index >= 0:
                maxbone.parent = maxbones[b.parent_index]
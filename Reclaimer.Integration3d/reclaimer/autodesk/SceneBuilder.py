from typing import cast
from typing import Dict, Tuple, List
from functools import reduce
import operator

from pymxs import runtime as rt

from ..src.ImportOptions import *
from ..src.Scene import *
from ..src.Model import *
from ..src.Material import *
from ..src.Types import *
from .Utils import *

__all__ = [
    'create_scene'
]

MX_UNITS = 100.0 # 1 max unit = 100mm?

UNIT_SCALE: float = 1.0
OPTIONS: ImportOptions = ImportOptions()

def create_scene(scene: Scene, options: ImportOptions = None):
    global UNIT_SCALE, OPTIONS
    UNIT_SCALE = scene.unit_scale / MX_UNITS
    # OPTIONS = options

    print(f'scene name: {scene.name}')
    print(f'scene scale: {scene.unit_scale}')

    for model in scene.model_pool:
        builder = ModelBuilder(scene, model)
        if OPTIONS.IMPORT_BONES and model.bones:
            print('creating skeleton')
            builder.create_bones()
        # if OPTIONS.IMPORT_MESHES and model.meshes:
        #     print('creating meshes')
        #     builder.create_meshes()
        # if OPTIONS.IMPORT_MARKERS and model.markers:
        #     print('creating markers')
        #     builder.create_markers()

def _convert_transform_units(transform: Matrix4x4, bone_mode: bool = False) -> rt.Matrix3:
    ''' Converts a transform from model units to max units '''
    if not bone_mode:
        return toMatrix3(transform) * rt.scaleMatrix(rt.Point3(UNIT_SCALE, UNIT_SCALE, UNIT_SCALE))

    # for bones we want to keep the scale component at 1x, but still need to convert the translation component
    m = toMatrix3(transform)
    return rt.preRotate(rt.transMatrix(m.translationPart * UNIT_SCALE), m.rotationPart)


class ModelBuilder:
    _scene: Scene
    _model: Model

    def __init__(self, scene: Scene, model: Model):
        self._scene = scene
        self._model = model

    def _get_bone_transforms(self) -> List[rt.Matrix3]:
        result = []
        for bone in self._model.bones:
            lineage = self._model.get_bone_lineage(bone)
            transforms = [_convert_transform_units(x.transform, True) for x in reversed(lineage)]
            result.append(reduce(operator.mul, transforms))
        return result

    def create_bones(self):
        scene, model = self._scene, self._model

        BONE_SIZE = 0.03 * UNIT_SCALE * OPTIONS.BONE_SCALE
        TAIL_VECTOR = rt.Point3(BONE_SIZE, 0.0, 0.0)

        bone_transforms = self._get_bone_transforms()

        maxbones = []
        for i, b in enumerate(model.bones):
            maxbone = rt.BoneSys.createBone(rt.Point3(0, 0, 0), TAIL_VECTOR, rt.Point3(0, 0, 1))
            maxbone.setBoneEnable(False, 0)
            maxbone.name = OPTIONS.bone_name(b)
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

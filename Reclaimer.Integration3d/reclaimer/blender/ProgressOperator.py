import bpy
from typing import cast

from .QtWindowEventLoop import QtWindowEventLoop

from .. import ui
from ..ui.ProgressDialog import ProgressDialog
from ..src.Scene import *
from ..src.SceneFilter import *
from ..src.ImportOptions import *


class RmfProgressOperator(QtWindowEventLoop):
    '''RMF Import Progress'''

    bl_idname: str = 'rmf.progress_operator'
    bl_label: str = 'RMF Import Progress'

    def create_dialog(self):
        data = bpy.types.Scene.rmf_data

        scene = cast(Scene, data['scene'])
        filter = cast(SceneFilter, data['filter'])
        options = cast(ImportOptions, data['options'])

        dialog = ProgressDialog(scene, filter, options, stylesheet=ui.resource('bl_stylesheet.qss'))
        data['progress'] = dialog
        return dialog

    def dialog_closed(self):
        del bpy.types.Scene.rmf_data

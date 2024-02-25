import bpy
from typing import cast

from .QtWindowEventLoop import QtWindowEventLoop

from .BlenderInterface import *
from .. import ui
from ..ui.ProgressDialog import ProgressDialog
from ..src.Scene import *
from ..src.SceneFilter import *
from ..src.ImportOptions import *
from ..src.SceneBuilder import *


class RmfProgressOperator(QtWindowEventLoop):
    '''RMF Import Progress'''

    bl_idname: str = 'rmf.progress_operator'
    bl_label: str = 'RMF Import Progress'

    def create_dialog(self):
        data = bpy.types.Scene.rmf_data

        scene = cast(Scene, data['scene'])
        filter = cast(SceneFilter, data['filter'])
        options = cast(ImportOptions, data['options'])
        del bpy.types.Scene.rmf_data

        dialog = ProgressDialog(scene, filter, options, stylesheet=ui.resource('bl_stylesheet.qss'))

        interface = BlenderInterface()
        builder = SceneBuilder(interface, scene, filter, options, dialog)
        task_queue = builder.begin_create_scene()

        def execute_next():
            if task_queue.finished() or dialog.cancel_requested:
                builder.end_create_scene()
                return

            task_queue.execute_batch()
            return 0

        bpy.app.timers.register(execute_next, first_interval=0.1)
        return dialog
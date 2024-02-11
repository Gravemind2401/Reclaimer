import bpy
from typing import cast
from PySide2 import QtWidgets

from . import SceneBuilder
from .QtWindowEventLoop import QtWindowEventLoop
from .. import ui
from ..ui.RmfDialog import RmfDialog


class RmfDialogOperator(QtWindowEventLoop):
    '''Import an RMF file'''

    bl_idname: str = 'rmf.dialog_operator'
    bl_label: str = 'Import RMF'

    filepath: bpy.props.StringProperty(subtype='FILE_PATH')

    def create_dialog(self):
        return RmfDialog(self.filepath, stylesheet=ui.resource('bl_stylesheet.qss'))

    def dialog_closed(self):
        dialog = cast(RmfDialog, self.dialog)
        if dialog.result() != QtWidgets.QDialog.DialogCode.Accepted:
            return

        scene, filter, options = dialog.get_import_options()
        bpy.types.Scene.rmf_data = {
            'scene': scene,
            'filter': filter,
            'options': options
        }

        bpy.ops.rmf.progress_operator('EXEC_DEFAULT')
        progress_callback = bpy.types.Scene.rmf_data['progress']

        SceneBuilder.create_scene(scene, filter, options, progress_callback)

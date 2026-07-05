import os
import bpy
from typing import cast

from .QtWindowEventLoop import QtWindowEventLoop
from .. import ui
from ..ui.RmfDialog import RmfDialog

qt_binding = os.environ.get('QT_PREFERRED_BINDING', 'PySide2')
if qt_binding == 'PySide6':
    from PySide6 import QtWidgets
else:
    from PySide2 import QtWidgets


class RmfDialogOperator(QtWindowEventLoop):
    '''Import an RMF file'''

    bl_idname: str = 'rmf.dialog_operator'
    bl_label: str = 'Import RMF'

    filepath: bpy.props.StringProperty(
        subtype = 'FILE_PATH'
    ) # type: ignore

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

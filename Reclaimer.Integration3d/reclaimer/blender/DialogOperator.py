import bpy
from .QtWindowEventLoop import QtWindowEventLoop
from PySide2 import QtWidgets
from PySide2.QtUiTools import QUiLoader

from . import SceneBuilder
from .. import ui
from ..ui.RmfDialogManager import RmfDialogManager


class RmfDialogOperator(QtWindowEventLoop):
    '''Import an RMF file'''

    bl_idname: str = 'rmf.dialog_operator'
    bl_label: str = 'Import RMF'

    filepath: bpy.props.StringProperty(subtype='FILE_PATH')

    def __init__(self):
        loader = QUiLoader()
        super().__init__(loader.load, ui.MAIN_UI_FILE, None)

    def init_widget(self, widget: QtWidgets.QWidget):
        self._manager = RmfDialogManager(widget, self.filepath)
        self._manager.dialog.show()

    def exit_widget(self, widget: QtWidgets.QWidget):
        dialog = self._manager.dialog # should be the same instance as 'widget'
        if dialog.result() == QtWidgets.QDialog.DialogCode.Accepted:
            SceneBuilder.create_scene(self._manager._scene, None, self._manager.get_import_options())

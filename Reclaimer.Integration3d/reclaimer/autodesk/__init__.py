from pymxs import runtime as rt
from PySide2 import QtWidgets
from PySide2.QtWidgets import QWidget

from . import SceneBuilder
from ..ui.RmfDialog import RmfDialog


def import_rmf():
    filepath = rt.getOpenFileName(types="RMF Files (*.rmf)|*.rmf")
    if not filepath:
        return

    main_window = QWidget.find(rt.Windows.getMaxHWND())
    dlg = MaxRmfDialog(filepath, main_window)
    dlg.show()


class MaxRmfDialog(RmfDialog):
    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        if result != QtWidgets.QDialog.DialogCode.Accepted:
            return

        scene, filter, options = self.get_import_options()
        SceneBuilder.create_scene(scene, filter, options)
        rt.completeRedraw()
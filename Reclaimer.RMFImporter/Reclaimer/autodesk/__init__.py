import os, inspect
import pymxs
from pymxs import runtime as rt

os.environ.setdefault('QT_PREFERRED_BINDING', 'PySide6' if pymxs.runtime.maxversion()[7] >= 2025 else 'PySide2')

qt_binding = os.environ.get('QT_PREFERRED_BINDING', 'PySide2')
if qt_binding == 'PySide6':
    from PySide6 import QtWidgets
    from PySide6.QtWidgets import QWidget
else:
    from PySide2 import QtWidgets
    from PySide2.QtWidgets import QWidget

from .AutodeskInterface import AutodeskInterface
from ..src.SceneBuilder import SceneBuilder
from ..ui.RmfDialog import RmfDialog

AUTODESK_ROOT = os.path.dirname(inspect.getabsfile(inspect.currentframe()))
RESOURCE_ROOT = os.path.join(AUTODESK_ROOT, 'resources')

def resource(name: str):
    return os.path.join(RESOURCE_ROOT, name)

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

        interface = AutodeskInterface()
        builder = SceneBuilder(interface, scene, filter, options)

        error: Exception = None

        with pymxs.animate(False):
            with pymxs.undo(False):
                # if an unhandled exception happens inside the animate/undo context
                # then it will not revert the context, so we need to catch and re-throw
                try:
                    task_queue = builder.begin_create_scene()
                    while not task_queue.finished():
                        task_queue.execute_next()
                    error = task_queue.error
                except Exception as e:
                    error = e

        builder.end_create_scene()
        rt.completeRedraw()

        if error:
            raise error
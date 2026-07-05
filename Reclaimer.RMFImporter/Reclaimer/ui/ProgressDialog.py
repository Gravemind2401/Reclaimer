import os
from pathlib import Path
from typing import cast, Optional

from .. import ui
from ..src.Progress import *
from ..src.Scene import *
from ..src.SceneFilter import *
from ..src.ImportOptions import *

compiled_ui = False
qt_binding = os.environ.get('QT_PREFERRED_BINDING', 'PySide2')
if qt_binding == 'PySide6':
    from PySide6 import QtCore, QtWidgets
    from PySide6.QtUiTools import QUiLoader
    from ..ui.progress_ui import Ui_Form as FormLoader
    compiled_ui = True
else:
    from PySide2 import QtCore, QtWidgets
    from PySide2.QtUiTools import QUiLoader

__all__ = [
    'ProgressDialog'
]


class ProgressDialog(ProgressCallback, QtWidgets.QDialog):
    _scene: Scene
    _scene_filter: SceneFilter
    _widget: QtWidgets.QWidget

    def __init__(self, scene: Scene, filter: SceneFilter, options: ImportOptions, parent: Optional[QtWidgets.QWidget] = None, flags: QtCore.Qt.WindowFlags = QtCore.Qt.WindowFlags(), stylesheet: Optional[str] = None):
        ProgressCallback.__init__(self, filter, options)
        QtWidgets.QDialog.__init__(self, parent, flags)

        if compiled_ui:
            container = QtWidgets.QWidget()
            widget = self._widget = FormLoader()
            widget.setupUi(container)
        else:
            loader = QUiLoader() # this crashes blender on PySide6 for some reason
            container = widget = self._widget = loader.load(ui.PROGRESS_UI_FILE, None)

        if stylesheet:
            ui.set_stylesheet(container, stylesheet)

        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        self.setLayout(layout)
        layout.addWidget(container)

        self.setWindowIcon(ui.create_icon('Settings_16x.png'))
        self.setWindowTitle(Path(scene._source_file).name)
        self.setModal(True)
        self.setWindowFlags(QtCore.Qt.Dialog | QtCore.Qt.MSWindowsFixedSizeDialogHint)
        self.setWindowFlag(QtCore.Qt.WindowContextHelpButtonHint, False)

        if not options.IMPORT_MATERIALS:
            cast(QtWidgets.QLayout, self._widget.materials_layout).setEnabled(False)

        if not options.IMPORT_MESHES:
            cast(QtWidgets.QLayout, self._widget.meshes_layout).setEnabled(False)

        self._widget.progressBar_materials.setMaximum(self.material_count)
        self._widget.progressBar_meshes.setMaximum(self.mesh_count)
        self._widget.progressBar_objects.setMaximum(self.object_count)
        self._refresh()

        self._connect()

    def complete(self):
        self.accept()

    def _refresh(self):
        self._widget.progressBar_materials.setValue(self.material_progress)
        self._widget.progressBar_meshes.setValue(self.mesh_progress)
        self._widget.progressBar_objects.setValue(self.object_progress)

        self._widget.label_materials_progress.setText(f'{self.material_progress} / {self.material_count} ({self.material_percent:.0%})')
        self._widget.label_meshes_progress.setText(f'{self.mesh_progress} / {self.mesh_count} ({self.mesh_percent:.0%})')
        self._widget.label_objects_progress.setText(f'{self.object_progress} / {self.object_count} ({self.object_percent:.0%})')

    def _connect(self):
        self._widget.buttonBox.rejected.connect(self.reject)
        self.finished.connect(self.onDialogResult)

    def reject(self):
        self.cancel_requested = True
        return super().reject()

    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        ''' Override in a base class to be notified when the dialog result is received '''
        print(f'objects: {self.object_progress} / {self.object_count}')
        print(f'materials: {self.material_progress} / {self.material_count}')
        print(f'meshes: {self.mesh_progress} / {self.mesh_count}')
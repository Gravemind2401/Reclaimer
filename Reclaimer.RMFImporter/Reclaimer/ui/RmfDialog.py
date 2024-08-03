from pathlib import Path
from typing import Any, Tuple, cast, Iterator, Optional

from Reclaimer import package_version_string

from .. import ui
from ..src.SceneReader import SceneReader
from ..src.Scene import *
from ..src.Model import *
from ..src.ImportOptions import *
from ..src.SceneFilter import *

from PySide2 import QtCore, QtWidgets
from PySide2.QtUiTools import QUiLoader

__all__ = [
    'RmfDialog'
]

CheckState = QtCore.Qt.CheckState
CheckStates = [CheckState.Unchecked, CheckState.PartiallyChecked, CheckState.Checked]

def _enumerate_children_recursive(tree: QtWidgets.QTreeWidget) -> Iterator['CustomTreeItem']:
    def enumerate_recursive(item: QtWidgets.QTreeWidgetItem):
        yield item
        for i in range(item.childCount()):
            yield from enumerate_recursive(item.child(i))

    for i in range(tree.topLevelItemCount()):
        yield from enumerate_recursive(tree.topLevelItem(i))


class CustomTreeItem(QtWidgets.QTreeWidgetItem):
    SYSTEM_TYPE: int = 0
    USER_TYPE: int = 1000

    _isRefreshing: bool = False
    dataContext: IFilterNode

    def __init__(self, parent, data_context: IFilterNode):
        super().__init__(parent, type=CustomTreeItem.USER_TYPE)
        self.dataContext = data_context
        self._refreshState()

    def setData(self, column: int, role: int, value: Any):
        ''' Override setData() to push UI state to underlying model when checkState changes '''

        super().setData(column, role, value)

        if self._isRefreshing or column != 0 or role != QtCore.Qt.CheckStateRole:
            return

        self.dataContext.toggle(int(self.checkState(0)))

        # refresh ancestors
        p = self.parent()
        while isinstance(p, CustomTreeItem):
            p._refreshState()
            p = p.parent()

        # refresh descendants
        for c in self._enumerateDescendants():
            c._refreshState()

    def _enumerateDescendants(self) -> Iterator['CustomTreeItem']:
        for i in range(self.childCount()):
            c = self.child(i)
            if isinstance(c, CustomTreeItem):
                yield c
                yield from CustomTreeItem._enumerateDescendants(c)

    def _refreshState(self):
        ''' Update UI state to match underlying model '''
        self._isRefreshing = True
        self.setCheckState(0, CheckStates[self.dataContext.state.value])
        self._isRefreshing = False


class RmfDialog(QtWidgets.QDialog):
    _error: Exception = None
    _scene: Scene
    _scene_filter: SceneFilter
    _widget: QtWidgets.QWidget
    _objectTreeWidget: QtWidgets.QTreeWidget
    _permTreeWidget: QtWidgets.QTreeWidget

    @property
    def _current_tree(self) -> QtWidgets.QTreeWidget:
        idx = cast(QtWidgets.QTabWidget, self._widget.tabWidget).currentIndex()
        return self._objectTreeWidget if idx == 0 else self._permTreeWidget

    def __init__(self, filepath: str, parent: Optional[QtWidgets.QWidget] = None, flags: QtCore.Qt.WindowFlags = QtCore.Qt.WindowFlags(), stylesheet: Optional[str] = None):
        super().__init__(parent, flags)
        loader = QUiLoader()

        widget = self._widget = loader.load(ui.WIDGET_UI_FILE, None)
        widget.toolButton_expandAll.setIcon(ui.create_icon('ExpandAll_16x.png'))
        widget.toolButton_collapseAll.setIcon(ui.create_icon('CollapseGroup_16x.png'))
        widget.toolButton_checkAll.setIcon(ui.create_icon('Checklist_16x.png'))
        widget.toolButton_uncheckAll.setIcon(ui.create_icon('CheckboxList_16x.png'))

        if stylesheet:
            ui.set_stylesheet(widget, stylesheet)

        self._objectTreeWidget = cast(QtWidgets.QTreeWidget, widget.objectTreeWidget)
        self._permTreeWidget = cast(QtWidgets.QTreeWidget, widget.permutationTreeWidget)

        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        self.setLayout(layout)
        layout.addWidget(widget)

        self.setWindowIcon(ui.create_icon('Settings_16x.png'))
        self.setWindowTitle(f'RMF Importer {package_version_string} - {Path(filepath).name}')
        self.setModal(True)
        self.setWindowFlags(QtCore.Qt.Dialog | QtCore.Qt.MSWindowsFixedSizeDialogHint)
        self.setWindowFlag(QtCore.Qt.WindowContextHelpButtonHint, False)

        try:
            self._scene = SceneReader.open_scene(filepath)
        except Exception as e:
            self._error = e
            return

        self._scene_filter = SceneFilter(self._scene)

        for tree in [self._objectTreeWidget, self._permTreeWidget]:
            self._create_hierarchy(tree)

        # only expand top level items to begin with
        # sort by name from level 2 onwards
        for tree in [self._objectTreeWidget, self._permTreeWidget]:
            tree.collapseAll()
            for item in self._enumerate_toplevel_items(tree):
                item.setExpanded(True)
                item.sortChildren(0, QtCore.Qt.SortOrder.AscendingOrder)

            tree.header().setSectionResizeMode(0, QtWidgets.QHeaderView.Stretch)
            self.check_all(tree, CheckState.Checked)

        self._load_options(ImportOptions())
        self._connect()

    def _load_options(self, options: ImportOptions):
        self._widget.checkBox_importBones.setChecked(options.IMPORT_BONES)
        self._widget.checkBox_importMarkers.setChecked(options.IMPORT_MARKERS)
        self._widget.checkBox_importMeshes.setChecked(options.IMPORT_MESHES)
        self._widget.checkBox_importMaterials.setChecked(options.IMPORT_MATERIALS)

        self._widget.checkBox_splitMeshes.setChecked(options.SPLIT_MESHES)
        self._widget.checkBox_importNormals.setChecked(options.IMPORT_NORMALS)
        self._widget.checkBox_importWeights.setChecked(options.IMPORT_SKIN)

        self._widget.lineEdit_bitmapsFolder.setText(options.BITMAP_ROOT)
        self._widget.lineEdit_bitmapsExtension.setText(options.BITMAP_EXT)

        self._widget.spinBox_objectScale.setValue(options.OBJECT_SCALE)
        self._widget.spinBox_boneScale.setValue(options.BONE_SCALE)
        self._widget.spinBox_markerScale.setValue(options.MARKER_SCALE)

    def _connect(self):
        self._widget.toolButton_expandAll.clicked.connect(lambda: self._current_tree.expandAll())
        self._widget.toolButton_collapseAll.clicked.connect(lambda: self._current_tree.collapseAll())
        self._widget.toolButton_checkAll.clicked.connect(lambda: self.check_all(self._current_tree, CheckState.Checked))
        self._widget.toolButton_uncheckAll.clicked.connect(lambda: self.check_all(self._current_tree, CheckState.Unchecked))
        self._widget.toolButton_bitmapsFolder.clicked.connect(self._browseBitmaps)
        self._widget.tabWidget.currentChanged.connect(self._onTabChanged)
        self._widget.buttonBox.accepted.connect(self.accept)
        self._widget.buttonBox.rejected.connect(self.reject)
        self.finished.connect(self.onDialogResult)

    def _browseBitmaps(self):
        dir = self._widget.lineEdit_bitmapsFolder.text()
        dir = QtWidgets.QFileDialog.getExistingDirectory(self, caption='Select bitmaps folder', dir=dir)
        if dir:
            self._widget.lineEdit_bitmapsFolder.setText(dir)

    def _onTabChanged(self, index: int):
        for item in _enumerate_children_recursive(self._current_tree):
            item._refreshState()

    def _enumerate_toplevel_items(self, tree: QtWidgets.QTreeWidget) -> Iterator['CustomTreeItem']:
        for i in range(tree.topLevelItemCount()):
            yield tree.topLevelItem(i)

    def check_all(self, tree: QtWidgets.QTreeWidget, state: CheckState):
        for item in self._enumerate_toplevel_items(tree):
            item.setCheckState(0, state)

    def _create_hierarchy(self, tree: QtWidgets.QTreeWidget):
        def build_treeitem(parent: Any, node: IFilterNode) -> CustomTreeItem:
            item = CustomTreeItem(parent, node)
            item.setText(0, node.label)
            item.setText(1, node.node_type)

            if type(node) == FilterGroup:
                item.addChildren([build_treeitem(item, o) for o in node.groups])
                item.addChildren([build_treeitem(item, o) for o in node.models])

            if tree == self._objectTreeWidget:
                if type(node) == ModelFilter:
                    item.addChildren([build_treeitem(item, o) for o in node.regions])

                if type(node) == RegionFilter:
                    item.addChildren([build_treeitem(item, o) for o in node.permutations])
            else:
                if type(node) == ModelFilter:
                    item.addChildren([build_treeitem(item, o) for o in node.permutation_sets])

            return item

        tree.clear()
        tree.addTopLevelItems([build_treeitem(tree, o) for o in self._scene_filter.groups])
        tree.addTopLevelItems([build_treeitem(tree, o) for o in self._scene_filter.models])

    def get_import_options(self) -> Tuple[Scene, SceneFilter, ImportOptions]:
        options = ImportOptions(self._scene)

        options.IMPORT_BONES = self._widget.checkBox_importBones.isChecked()
        options.IMPORT_MARKERS = self._widget.checkBox_importMarkers.isChecked()
        options.IMPORT_MESHES = self._widget.checkBox_importMeshes.isChecked()
        options.IMPORT_MATERIALS = self._widget.checkBox_importMaterials.isChecked()

        options.SPLIT_MESHES = self._widget.checkBox_splitMeshes.isChecked()
        options.IMPORT_NORMALS = self._widget.checkBox_importNormals.isChecked()
        options.IMPORT_SKIN = self._widget.checkBox_importWeights.isChecked()

        options.BITMAP_ROOT = self._widget.lineEdit_bitmapsFolder.text()
        options.BITMAP_EXT = self._widget.lineEdit_bitmapsExtension.text()

        options.OBJECT_SCALE = self._widget.spinBox_objectScale.value()
        options.BONE_SCALE = self._widget.spinBox_boneScale.value()
        options.MARKER_SCALE = self._widget.spinBox_markerScale.value()

        return (self._scene, self._scene_filter, options)

    def show(self):
        super().show()

        if self._error:
            QtWidgets.QMessageBox.critical(self, 'Error', f'Error reading file: {self._error}\n\nTry saving the file again.')
            self.close()
            self.onDialogResult(QtWidgets.QDialog.DialogCode.Rejected)

    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        ''' Override in a base class to be notified when the dialog result is received '''
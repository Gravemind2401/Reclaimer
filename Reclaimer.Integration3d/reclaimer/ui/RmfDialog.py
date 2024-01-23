from typing import Any, Tuple, cast, Iterator, Optional

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
    _scene: Scene
    _scene_filter: SceneFilter
    _widget: QtWidgets.QWidget
    _treeWidget: QtWidgets.QTreeWidget

    def __init__(self, filepath: str, parent: Optional[QtWidgets.QWidget] = None, flags: QtCore.Qt.WindowFlags = QtCore.Qt.WindowFlags()):
        super().__init__(parent, flags)
        loader = QUiLoader()

        widget = self._widget = loader.load(ui.WIDGET_UI_FILE, None)
        self._treeWidget = cast(QtWidgets.QTreeWidget, widget.treeWidget)

        layout = QtWidgets.QVBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        self.setLayout(layout)
        layout.addWidget(widget)

        self.setWindowTitle(filepath)
        self.setModal(True)

        self._scene = SceneReader.open_scene(filepath)
        self._scene_filter = SceneFilter(self._scene)

        self._treeWidget.clear()
        self._create_hierarchy()

        # only expand top level items to begin with
        # sort by name from level 2 onwards
        self._treeWidget.collapseAll()
        for item in self._enumerate_toplevel_items():
            item.setExpanded(True)
            item.sortChildren(0, QtCore.Qt.SortOrder.AscendingOrder)

        self._treeWidget.header().setSectionResizeMode(0, QtWidgets.QHeaderView.Stretch)
        self.check_all(CheckState.Checked)

        self._connect()

    def _connect(self):
        self._widget.toolButton_checkAll.clicked.connect(lambda: self.check_all(CheckState.Checked))
        self._widget.toolButton_uncheckAll.clicked.connect(lambda: self.check_all(CheckState.Unchecked))
        self._widget.buttonBox.accepted.connect(self.accept)
        self._widget.buttonBox.rejected.connect(self.reject)
        self.finished.connect(self.onDialogResult)

    def sizeHint(self) -> QtCore.QSize:
        size = QtCore.QSize()
        size.setWidth(490)
        size.setHeight(450)
        return size

    def _enumerate_toplevel_items(self) -> Iterator['CustomTreeItem']:
        for i in range(self._treeWidget.topLevelItemCount()):
            yield self._treeWidget.topLevelItem(i)

    def check_all(self, state: CheckState):
        for item in self._enumerate_toplevel_items():
            item.setCheckState(0, state)

    def _create_hierarchy(self):
        def build_treeitem(parent: Any, node: IFilterNode) -> CustomTreeItem:
            item = CustomTreeItem(parent, node)
            item.setText(0, node.label)
            item.setText(1, node.node_type)

            if type(node) == FilterGroup:
                item.addChildren([build_treeitem(item, o) for o in node.groups])
                item.addChildren([build_treeitem(item, o) for o in node.models])

            if type(node) == ModelFilter:
                item.addChildren([build_treeitem(item, o) for o in node.regions])

            if type(node) == RegionFilter:
                item.addChildren([build_treeitem(item, o) for o in node.permutations])

            return item

        self._treeWidget.addTopLevelItems([build_treeitem(self._treeWidget, o) for o in self._scene_filter.groups])
        self._treeWidget.addTopLevelItems([build_treeitem(self._treeWidget, o) for o in self._scene_filter.models])

    def get_import_options(self) -> Tuple[SceneFilter, ImportOptions]:
        options = ImportOptions()

        options.IMPORT_BONES = self._widget.checkBox_importBones.isChecked()
        options.IMPORT_MARKERS = self._widget.checkBox_importMarkers.isChecked()
        options.IMPORT_MESHES = self._widget.checkBox_importMeshes.isChecked()
        options.IMPORT_MATERIALS = self._widget.checkBox_importMaterials.isChecked()

        options.SPLIT_MESHES = self._widget.checkBox_splitMeshes.isChecked()
        options.IMPORT_NORMALS = self._widget.checkBox_importNormals.isChecked()
        options.IMPORT_SKIN = self._widget.checkBox_importWeights.isChecked()

        options.OBJECT_SCALE = self._widget.spinBox_objectScale.value()
        options.BONE_SCALE = self._widget.spinBox_boneScale.value()
        options.MARKER_SCALE = self._widget.spinBox_markerScale.value()

        return (self._scene_filter, options)

    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        ''' Override in a base class to be notified when the dialog result is received '''
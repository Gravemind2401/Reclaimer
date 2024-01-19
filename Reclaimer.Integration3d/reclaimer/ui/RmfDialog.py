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


class CustomTreeItem(QtWidgets.QTreeWidgetItem):
    SYSTEM_TYPE: int = 0
    USER_TYPE: int = 1000

    _isRefreshing: bool = False
    dataContext: IFilterNode

    def __init__(self, parent, data_context: IFilterNode):
        super().__init__(parent, type=CustomTreeItem.USER_TYPE)
        self.dataContext = data_context
        self.setCheckState(0, CheckState.Unchecked)

    def setData(self, column: int, role: int, value: Any):
        ''' Override setData() to automatically keep check state in sync between parent/child '''

        super().setData(column, role, value)
        self.dataContext.selected = self.checkState(0) != CheckState.Unchecked

        if not self._isRefreshing and column == 0 and role == QtCore.Qt.CheckStateRole:
            self._isRefreshing = True

            self._refreshAncestors()
            self._refreshDescendents()

            self._isRefreshing = False

    def enumerateChildren(self) -> Iterator['CustomTreeItem']:
        for i in range(self.childCount()):
            yield self.child(i)

    def _setCheckStateQuiet(self, state: CheckState):
        ''' Set own CheckState without causing a refresh '''

        self._isRefreshing = True
        self.setCheckState(0, state)
        self._isRefreshing = False

    def _refreshDescendents(self):
        ''' Show/hide children depending on own CheckState '''

        state = self.checkState(0)
        if state == CheckState.Unchecked:
            self.setExpanded(False)
        for c in self.enumerateChildren():
            c.setHidden(state == CheckState.Unchecked)
            if state != CheckState.PartiallyChecked:
                c.setCheckState(0, state)

    def _refreshAncestors(self):
        ''' Update parent CheckState depending on own/sibling states'''

        parent = self.parent()
        if type(parent) != CustomTreeItem or parent._isRefreshing:
            return

        states = set(o.checkState(0) for o in parent.enumerateChildren() if not o.isHidden())
        if len(states) == 0:
            parent._setCheckStateQuiet(CheckState.Unchecked)
        elif len(states) > 1:
            parent._setCheckStateQuiet(CheckState.PartiallyChecked)
        else:
            parent._setCheckStateQuiet(states.pop())

        parent._refreshAncestors()


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

        widget.buttonBox.accepted.connect(self.accept)
        widget.buttonBox.rejected.connect(self.reject)
        self.finished.connect(self.onDialogResult)

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

        self._treeWidget.resizeColumnToContents(0)
        self.check_all(CheckState.Checked)

    def sizeHint(self) -> QtCore.QSize:
        size = QtCore.QSize()
        size.setWidth(370)
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
        options = ImportOptions() # TODO: get settings based on UI state
        return (self._scene_filter, options)

    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        ''' Override in a base class to be notified when the dialog result is received '''
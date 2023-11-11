from typing import Any, cast, Iterator

from ..src.SceneReader import SceneReader
from ..src.Scene import *
from ..src.Model import *
from ..src.ImportOptions import *

from PySide2 import QtCore, QtWidgets

__all__ = [
    'RmfDialogManager'
]

CheckState = QtCore.Qt.CheckState


class CustomTreeItem(QtWidgets.QTreeWidgetItem):
    SYSTEM_TYPE: int = 0
    USER_TYPE: int = 1000

    _isRefreshing: bool = False
    dataContext: Any

    def __init__(self, parent, data_context):
        super().__init__(parent, type=CustomTreeItem.USER_TYPE)
        self.dataContext = data_context
        self.setCheckState(0, CheckState.Unchecked)

    def setData(self, column: int, role: int, value: Any):
        ''' Override setData() to automatically keep check state in sync between parent/child '''

        super().setData(column, role, value)
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


class RmfDialogManager():
    __TYPENAMES = {
        SceneGroup: 'Group',
        Placement: 'Placement',
        ModelRef: 'Model',
        Model: 'Model',
        ModelRegion: 'Region',
        ModelPermutation: 'Permutation'
    }

    dialog: QtWidgets.QDialog
    _scene: Scene
    _treeWidget: QtWidgets.QTreeWidget

    def __init__(self, dialog: QtWidgets.QDialog, filepath: str) -> None:
        self.dialog = dialog
        self._scene = SceneReader.open_scene(filepath)
        self._treeWidget = cast(QtWidgets.QTreeWidget, dialog.treeWidget)

        dialog.setWindowTitle(filepath)
        self._treeWidget.clear()

        self._connect()
        self._create_hierarchy()

        # only expand top level items to begin with
        # sort by name from level 2 onwards
        self._treeWidget.collapseAll()
        for i in range(self._treeWidget.topLevelItemCount()):
            item = self._treeWidget.topLevelItem(i)
            item.setExpanded(True)
            item.sortChildren(0, QtCore.Qt.SortOrder.AscendingOrder)

        self._treeWidget.resizeColumnToContents(0)
        self.check_all(CheckState.Checked)

    def check_all(self, state: CheckState):
        for i in range(self._treeWidget.topLevelItemCount()):
            self._treeWidget.topLevelItem(i).setCheckState(0, state)

    def _connect(self):
        self.dialog.finished.connect(self.onDialogResult)

    def _create_hierarchy(self):
        def build_treeitem(parent: Any, node: Any) -> CustomTreeItem:
            item = CustomTreeItem(parent, node)
            item.setText(1, RmfDialogManager.__TYPENAMES[type(node)])

            if type(node) == Placement and len(node.name) == 0:
                node = node.object

            if type(node) == ModelRef:
                node = self._scene.model_pool[node.model_index]

            item.setText(0, str(node))

            if type(node) == SceneGroup:
                item.addChildren([build_treeitem(item, o) for o in node.child_groups])
                item.addChildren([build_treeitem(item, o) for o in node.child_objects])

            return item

        self._treeWidget.addTopLevelItems([build_treeitem(self._treeWidget, o) for o in self._scene.root_node.child_groups])
        self._treeWidget.addTopLevelItems([build_treeitem(self._treeWidget, o) for o in self._scene.root_node.child_objects])

    def get_import_options(self) -> ImportOptions:
        # TODO: get settings and selected objects to import based on UI state
        return ImportOptions()

    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        ''' Override in a base class to be notified when the dialog result is received '''
import unittest
import sys

from PySide2 import QtWidgets
from PySide2.QtWidgets import QApplication

from .. import ui
from ..ui.RmfDialog import RmfDialog

FILEPATH = 'Z:\\data\\masterchief.rmf'

class TestDialog(RmfDialog):
    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        QtWidgets.QApplication.instance().exit()

class Test_PySide(unittest.TestCase):
    def test_pyside(self):
        app = QApplication(sys.argv)
        w = TestDialog(FILEPATH, stylesheet=ui.resource('bl_stylesheet.qss'))
        w.show()

if __name__ == '__main__':
    unittest.main()

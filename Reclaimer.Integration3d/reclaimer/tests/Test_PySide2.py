import unittest
import sys

from PySide2 import QtCore
from PySide2 import QtGui
from PySide2 import QtWidgets
from PySide2.QtWidgets import QApplication, QWidget, QMainWindow

from ..src.SceneReader import SceneReader
from ..ui.RmfDialog import RmfDialog

FILEPATH = 'Z:\\data\\masterchief.rmf'

class TestDialog(RmfDialog):
    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        QtWidgets.QApplication.instance().exit()

class Test_PySide(unittest.TestCase):
    def test_pyside(self):
        app = QApplication(sys.argv)
        w = TestDialog(FILEPATH)
        w.show()
        app.exec_()

if __name__ == '__main__':
    unittest.main()

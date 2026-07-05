import unittest
import os, sys

os.environ.setdefault('QT_PREFERRED_BINDING', 'PySide6')

from .. import ui
from ..ui.RmfDialog import RmfDialog


qt_binding = os.environ.get('QT_PREFERRED_BINDING', 'PySide2')
if qt_binding == 'PySide6':
    from PySide6 import QtWidgets
    from PySide6.QtWidgets import QApplication
else:
    from PySide2 import QtWidgets
    from PySide2.QtWidgets import QApplication

FILEPATH = 'Z:\\data\\masterchief.rmf'

class TestDialog(RmfDialog):
    def onDialogResult(self, result: QtWidgets.QDialog.DialogCode):
        QtWidgets.QApplication.instance().exit()

class Test_PySide(unittest.TestCase):
    def test_pyside(self):
        app = QApplication(sys.argv)
        w = TestDialog(FILEPATH, stylesheet=ui.resource('bl_stylesheet.qss'))
        w.show()
        app.exec_()

if __name__ == '__main__':
    unittest.main()

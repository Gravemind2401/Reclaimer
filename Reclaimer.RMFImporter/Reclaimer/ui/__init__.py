import os, inspect
from PySide2 import QtCore, QtWidgets, QtGui, QtSvg

UI_ROOT = os.path.dirname(inspect.getabsfile(inspect.currentframe()))
RESOURCE_ROOT = os.path.join(UI_ROOT, 'resources')
WIDGET_UI_FILE = os.path.join(UI_ROOT, 'widget.ui')
PROGRESS_UI_FILE = os.path.join(UI_ROOT, 'progress.ui')

def resource(name: str):
    return os.path.join(RESOURCE_ROOT, name)

def create_icon(name: str) -> QtGui.QIcon:
    filename = resource(name)
    if not name.endswith('.svg'):
        return QtGui.QIcon(filename)

    renderer = QtSvg.QSvgRenderer(filename)
    image = QtGui.QImage(16, 16, QtGui.QImage.Format_ARGB32)
    image.fill(0)
    renderer.render(QtGui.QPainter(image))
    pixmap = QtGui.QPixmap.fromImage(image)
    return QtGui.QIcon(pixmap)

def inject_resource_paths(stylesheet: str) -> str:
    '''
    This replaces the :/res/ urls in a stylesheet with full paths.
    This relies on there being no alias configured for the resource paths.
    Currently the qrc resource file is only being used for the Qt Designer.
    '''
    return stylesheet.replace(':/res', RESOURCE_ROOT.replace('\\', '/'))

def set_stylesheet(widget: QtWidgets.QWidget, filepath: str):
    file_qss = QtCore.QFile(filepath)
    if file_qss.exists():
        file_qss.open(QtCore.QFile.ReadOnly)
        stylesheet = QtCore.QTextStream(file_qss).readAll()
        stylesheet = inject_resource_paths(stylesheet)
        widget.setStyleSheet(stylesheet)
        file_qss.close()
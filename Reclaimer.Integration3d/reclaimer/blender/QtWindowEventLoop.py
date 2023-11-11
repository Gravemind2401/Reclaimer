# https://github.com/friedererdmann/blender_pyside2_example/tree/master

import sys
import os
import logging
import bpy

from typing import Set

__all__ = [
    'QtWindowEventLoop'
]

qt_binding = os.environ.get('QT_PREFERRED_BINDING') # correct qt bindings
if qt_binding:
    if qt_binding == 'PySide2':
        from PySide2 import QtWidgets, QtCore
    if qt_binding == 'PyQt5':
        from PyQt5 import QtWidgets, QtCore
else:
    from PySide2 import QtWidgets, QtCore

logger = logging.getLogger('qtutils')


class QtWindowEventLoop(bpy.types.Operator):
    ''' Allows PyQt or PySide to run inside Blender '''

    bl_idname = 'screen.qt_event_loop'
    bl_label = 'Qt Event Loop'

    execution_context: bpy.types.Context

    def __init__(self, widget, *args, **kwargs):
        self._widget = widget
        self._args = args
        self._kwargs = kwargs

    def modal(self, context: bpy.types.Context, event: bpy.types.Event):
        # bpy.context.window_manager
        wm = context.window_manager

        if not self.widget.isVisible():
            # if widget is closed
            logger.debug('finish modal operator')
            wm.event_timer_remove(self._timer)
            self.exit_widget(self.widget)
            return {'FINISHED'}
        else:
            logger.debug('process the events for Qt window')
            self.event_loop.processEvents()
            self.app.sendPostedEvents(None, 0)

        return {'PASS_THROUGH'}

    def execute(self, context: bpy.types.Context) -> Set[str]:
        logger.debug('execute operator')

        # instance() gives the possibility to have multiple windows
        # and close it one by one
        self.app = QtWidgets.QApplication.instance()

        if not self.app:
            # create the first instance
            self.app = QtWidgets.QApplication(sys.argv)

        if 'stylesheet' in self._kwargs:
            stylesheet = self._kwargs['stylesheet']
            self._set_stylesheet(self.app, stylesheet)

        self.event_loop = QtCore.QEventLoop()
        self.widget = self._widget(*self._args, **self._kwargs)

        self.init_widget(self.widget)

        logger.debug(self.app)
        logger.debug(self.widget)

        # run modal
        wm = context.window_manager
        self._timer = wm.event_timer_add(1 / 120, window=context.window)
        context.window_manager.modal_handler_add(self)

        return {'RUNNING_MODAL'}

    def _set_stylesheet(self, app, filepath):
        file_qss = QtCore.QFile(filepath)
        if file_qss.exists():
            file_qss.open(QtCore.QFile.ReadOnly)
            stylesheet = QtCore.QTextStream(file_qss).readAll()
            app.setStyleSheet(stylesheet)
            file_qss.close()

    def init_widget(self, widget: QtWidgets.QWidget):
        ''' Override in a base class to configure the widget during initialization '''

    def exit_widget(self, widget: QtWidgets.QWidget):
        ''' Override in a base class to run any finalization/cleanup code when the widget is closed '''

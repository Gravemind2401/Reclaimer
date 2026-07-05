# -*- coding: utf-8 -*-

################################################################################
## Form generated from reading UI file 'progress.ui'
##
## Created by: Qt User Interface Compiler version 6.11.1
##
## WARNING! All changes made in this file will be lost when recompiling UI file!
################################################################################

from PySide6.QtCore import (QCoreApplication, QDate, QDateTime, QLocale,
    QMetaObject, QObject, QPoint, QRect,
    QSize, QTime, QUrl, Qt)
from PySide6.QtGui import (QBrush, QColor, QConicalGradient, QCursor,
    QFont, QFontDatabase, QGradient, QIcon,
    QImage, QKeySequence, QLinearGradient, QPainter,
    QPalette, QPixmap, QRadialGradient, QTransform)
from PySide6.QtWidgets import (QAbstractButton, QApplication, QDialogButtonBox, QFrame,
    QLabel, QProgressBar, QSizePolicy, QVBoxLayout,
    QWidget)

class Ui_Form(object):
    def setupUi(self, Form):
        if not Form.objectName():
            Form.setObjectName(u"Form")
        Form.resize(460, 300)
        Form.setMinimumSize(QSize(460, 300))
        self.verticalLayout = QVBoxLayout(Form)
        self.verticalLayout.setObjectName(u"verticalLayout")
        self.materials_layout = QVBoxLayout()
        self.materials_layout.setObjectName(u"materials_layout")
        self.label_materials = QLabel(Form)
        self.label_materials.setObjectName(u"label_materials")
        self.label_materials.setAlignment(Qt.AlignCenter)

        self.materials_layout.addWidget(self.label_materials)

        self.progressBar_materials = QProgressBar(Form)
        self.progressBar_materials.setObjectName(u"progressBar_materials")
        self.progressBar_materials.setValue(24)
        self.progressBar_materials.setTextVisible(False)

        self.materials_layout.addWidget(self.progressBar_materials)

        self.label_materials_progress = QLabel(Form)
        self.label_materials_progress.setObjectName(u"label_materials_progress")
        self.label_materials_progress.setAlignment(Qt.AlignCenter)

        self.materials_layout.addWidget(self.label_materials_progress)

        self.line_2 = QFrame(Form)
        self.line_2.setObjectName(u"line_2")
        self.line_2.setFrameShape(QFrame.Shape.HLine)
        self.line_2.setFrameShadow(QFrame.Shadow.Sunken)

        self.materials_layout.addWidget(self.line_2)


        self.verticalLayout.addLayout(self.materials_layout)

        self.meshes_layout = QVBoxLayout()
        self.meshes_layout.setObjectName(u"meshes_layout")
        self.label_meshes = QLabel(Form)
        self.label_meshes.setObjectName(u"label_meshes")
        self.label_meshes.setAlignment(Qt.AlignCenter)

        self.meshes_layout.addWidget(self.label_meshes)

        self.progressBar_meshes = QProgressBar(Form)
        self.progressBar_meshes.setObjectName(u"progressBar_meshes")
        self.progressBar_meshes.setValue(24)
        self.progressBar_meshes.setTextVisible(False)

        self.meshes_layout.addWidget(self.progressBar_meshes)

        self.label_meshes_progress = QLabel(Form)
        self.label_meshes_progress.setObjectName(u"label_meshes_progress")
        self.label_meshes_progress.setAlignment(Qt.AlignCenter)

        self.meshes_layout.addWidget(self.label_meshes_progress)

        self.line_3 = QFrame(Form)
        self.line_3.setObjectName(u"line_3")
        self.line_3.setFrameShape(QFrame.Shape.HLine)
        self.line_3.setFrameShadow(QFrame.Shadow.Sunken)

        self.meshes_layout.addWidget(self.line_3)


        self.verticalLayout.addLayout(self.meshes_layout)

        self.objects_layout = QVBoxLayout()
        self.objects_layout.setObjectName(u"objects_layout")
        self.label_objects = QLabel(Form)
        self.label_objects.setObjectName(u"label_objects")
        self.label_objects.setAlignment(Qt.AlignCenter)

        self.objects_layout.addWidget(self.label_objects)

        self.progressBar_objects = QProgressBar(Form)
        self.progressBar_objects.setObjectName(u"progressBar_objects")
        self.progressBar_objects.setValue(24)
        self.progressBar_objects.setTextVisible(False)

        self.objects_layout.addWidget(self.progressBar_objects)

        self.label_objects_progress = QLabel(Form)
        self.label_objects_progress.setObjectName(u"label_objects_progress")
        self.label_objects_progress.setAlignment(Qt.AlignCenter)

        self.objects_layout.addWidget(self.label_objects_progress)

        self.line = QFrame(Form)
        self.line.setObjectName(u"line")
        self.line.setFrameShape(QFrame.Shape.HLine)
        self.line.setFrameShadow(QFrame.Shadow.Sunken)

        self.objects_layout.addWidget(self.line)


        self.verticalLayout.addLayout(self.objects_layout)

        self.buttonBox = QDialogButtonBox(Form)
        self.buttonBox.setObjectName(u"buttonBox")
        self.buttonBox.setStandardButtons(QDialogButtonBox.Cancel)
        self.buttonBox.setCenterButtons(True)

        self.verticalLayout.addWidget(self.buttonBox)


        self.retranslateUi(Form)

        QMetaObject.connectSlotsByName(Form)
    # setupUi

    def retranslateUi(self, Form):
        Form.setWindowTitle(QCoreApplication.translate("Form", u"Form", None))
        self.label_materials.setText(QCoreApplication.translate("Form", u"Materials", None))
        self.label_materials_progress.setText(QCoreApplication.translate("Form", u"0 / 100", None))
        self.label_meshes.setText(QCoreApplication.translate("Form", u"Meshes", None))
        self.label_meshes_progress.setText(QCoreApplication.translate("Form", u"0 / 100", None))
        self.label_objects.setText(QCoreApplication.translate("Form", u"Objects", None))
        self.label_objects_progress.setText(QCoreApplication.translate("Form", u"0 / 100", None))
    # retranslateUi


# -*- coding: utf-8 -*-

################################################################################
## Form generated from reading UI file 'widget.ui'
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
from PySide6.QtWidgets import (QAbstractButton, QAbstractItemView, QApplication, QCheckBox,
    QDialogButtonBox, QDoubleSpinBox, QFormLayout, QFrame,
    QGridLayout, QGroupBox, QHBoxLayout, QHeaderView,
    QLabel, QLineEdit, QSizePolicy, QSpacerItem,
    QTabWidget, QToolButton, QTreeWidget, QTreeWidgetItem,
    QVBoxLayout, QWidget)
from . import resources_rc

class Ui_Form(object):
    def setupUi(self, Form):
        if not Form.objectName():
            Form.setObjectName(u"Form")
        Form.resize(510, 578)
        Form.setMinimumSize(QSize(510, 500))
        self.gridLayout_4 = QGridLayout(Form)
        self.gridLayout_4.setSpacing(10)
        self.gridLayout_4.setObjectName(u"gridLayout_4")
        self.gridLayout_4.setContentsMargins(-1, -1, -1, 0)
        self.groupBox_importOptions = QGroupBox(Form)
        self.groupBox_importOptions.setObjectName(u"groupBox_importOptions")
        self.groupBox_importOptions.setCheckable(False)
        self.verticalLayout = QVBoxLayout(self.groupBox_importOptions)
        self.verticalLayout.setSpacing(8)
        self.verticalLayout.setObjectName(u"verticalLayout")
        self.checkBox_importBones = QCheckBox(self.groupBox_importOptions)
        self.checkBox_importBones.setObjectName(u"checkBox_importBones")
        self.checkBox_importBones.setChecked(True)

        self.verticalLayout.addWidget(self.checkBox_importBones)

        self.checkBox_importMarkers = QCheckBox(self.groupBox_importOptions)
        self.checkBox_importMarkers.setObjectName(u"checkBox_importMarkers")
        self.checkBox_importMarkers.setChecked(True)

        self.verticalLayout.addWidget(self.checkBox_importMarkers)

        self.checkBox_importMeshes = QCheckBox(self.groupBox_importOptions)
        self.checkBox_importMeshes.setObjectName(u"checkBox_importMeshes")
        self.checkBox_importMeshes.setChecked(True)

        self.verticalLayout.addWidget(self.checkBox_importMeshes)

        self.checkBox_importMaterials = QCheckBox(self.groupBox_importOptions)
        self.checkBox_importMaterials.setObjectName(u"checkBox_importMaterials")
        self.checkBox_importMaterials.setChecked(True)

        self.verticalLayout.addWidget(self.checkBox_importMaterials)


        self.gridLayout_4.addWidget(self.groupBox_importOptions, 0, 1, 1, 1)

        self.groupBox_scaleOptions = QGroupBox(Form)
        self.groupBox_scaleOptions.setObjectName(u"groupBox_scaleOptions")
        self.formLayout = QFormLayout(self.groupBox_scaleOptions)
        self.formLayout.setObjectName(u"formLayout")
        self.formLayout.setFieldGrowthPolicy(QFormLayout.AllNonFixedFieldsGrow)
        self.formLayout.setHorizontalSpacing(8)
        self.formLayout.setVerticalSpacing(8)
        self.label_boneScale = QLabel(self.groupBox_scaleOptions)
        self.label_boneScale.setObjectName(u"label_boneScale")

        self.formLayout.setWidget(1, QFormLayout.ItemRole.LabelRole, self.label_boneScale)

        self.spinBox_boneScale = QDoubleSpinBox(self.groupBox_scaleOptions)
        self.spinBox_boneScale.setObjectName(u"spinBox_boneScale")
        self.spinBox_boneScale.setValue(1.000000000000000)

        self.formLayout.setWidget(1, QFormLayout.ItemRole.FieldRole, self.spinBox_boneScale)

        self.label_markerScale = QLabel(self.groupBox_scaleOptions)
        self.label_markerScale.setObjectName(u"label_markerScale")

        self.formLayout.setWidget(2, QFormLayout.ItemRole.LabelRole, self.label_markerScale)

        self.spinBox_markerScale = QDoubleSpinBox(self.groupBox_scaleOptions)
        self.spinBox_markerScale.setObjectName(u"spinBox_markerScale")
        self.spinBox_markerScale.setValue(1.000000000000000)

        self.formLayout.setWidget(2, QFormLayout.ItemRole.FieldRole, self.spinBox_markerScale)

        self.spinBox_objectScale = QDoubleSpinBox(self.groupBox_scaleOptions)
        self.spinBox_objectScale.setObjectName(u"spinBox_objectScale")
        self.spinBox_objectScale.setValue(1.000000000000000)

        self.formLayout.setWidget(0, QFormLayout.ItemRole.FieldRole, self.spinBox_objectScale)

        self.label_objectScale = QLabel(self.groupBox_scaleOptions)
        self.label_objectScale.setObjectName(u"label_objectScale")

        self.formLayout.setWidget(0, QFormLayout.ItemRole.LabelRole, self.label_objectScale)


        self.gridLayout_4.addWidget(self.groupBox_scaleOptions, 3, 1, 1, 1)

        self.buttonBox = QDialogButtonBox(Form)
        self.buttonBox.setObjectName(u"buttonBox")
        self.buttonBox.setMinimumSize(QSize(0, 37))
        self.buttonBox.setStandardButtons(QDialogButtonBox.Cancel|QDialogButtonBox.Ok)
        self.buttonBox.setCenterButtons(True)

        self.gridLayout_4.addWidget(self.buttonBox, 5, 0, 1, 2)

        self.groupBox_filter = QGroupBox(Form)
        self.groupBox_filter.setObjectName(u"groupBox_filter")
        sizePolicy = QSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Expanding)
        sizePolicy.setHorizontalStretch(0)
        sizePolicy.setVerticalStretch(0)
        sizePolicy.setHeightForWidth(self.groupBox_filter.sizePolicy().hasHeightForWidth())
        self.groupBox_filter.setSizePolicy(sizePolicy)
        self.verticalLayout_3 = QVBoxLayout(self.groupBox_filter)
        self.verticalLayout_3.setObjectName(u"verticalLayout_3")
        self.toolboxFrame = QFrame(self.groupBox_filter)
        self.toolboxFrame.setObjectName(u"toolboxFrame")
        self.toolboxFrame.setFrameShape(QFrame.StyledPanel)
        self.horizontalLayout = QHBoxLayout(self.toolboxFrame)
        self.horizontalLayout.setObjectName(u"horizontalLayout")
        self.toolButton_expandAll = QToolButton(self.toolboxFrame)
        self.toolButton_expandAll.setObjectName(u"toolButton_expandAll")
        icon = QIcon()
        icon.addFile(u":/res/ExpandAll_16x.png", QSize(), QIcon.Mode.Normal, QIcon.State.Off)
        self.toolButton_expandAll.setIcon(icon)

        self.horizontalLayout.addWidget(self.toolButton_expandAll)

        self.toolButton_collapseAll = QToolButton(self.toolboxFrame)
        self.toolButton_collapseAll.setObjectName(u"toolButton_collapseAll")
        icon1 = QIcon()
        icon1.addFile(u":/res/CollapseGroup_16x.png", QSize(), QIcon.Mode.Normal, QIcon.State.Off)
        self.toolButton_collapseAll.setIcon(icon1)

        self.horizontalLayout.addWidget(self.toolButton_collapseAll)

        self.line = QFrame(self.toolboxFrame)
        self.line.setObjectName(u"line")
        self.line.setFrameShape(QFrame.Shape.VLine)
        self.line.setFrameShadow(QFrame.Shadow.Sunken)

        self.horizontalLayout.addWidget(self.line)

        self.toolButton_checkAll = QToolButton(self.toolboxFrame)
        self.toolButton_checkAll.setObjectName(u"toolButton_checkAll")
        icon2 = QIcon()
        icon2.addFile(u":/res/Checklist_16x.png", QSize(), QIcon.Mode.Normal, QIcon.State.Off)
        self.toolButton_checkAll.setIcon(icon2)

        self.horizontalLayout.addWidget(self.toolButton_checkAll)

        self.toolButton_uncheckAll = QToolButton(self.toolboxFrame)
        self.toolButton_uncheckAll.setObjectName(u"toolButton_uncheckAll")
        icon3 = QIcon()
        icon3.addFile(u":/res/CheckboxList_16x.png", QSize(), QIcon.Mode.Normal, QIcon.State.Off)
        self.toolButton_uncheckAll.setIcon(icon3)

        self.horizontalLayout.addWidget(self.toolButton_uncheckAll)

        self.horizontalSpacer = QSpacerItem(40, 20, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum)

        self.horizontalLayout.addItem(self.horizontalSpacer)


        self.verticalLayout_3.addWidget(self.toolboxFrame)

        self.tabWidget = QTabWidget(self.groupBox_filter)
        self.tabWidget.setObjectName(u"tabWidget")
        self.tabWidget.setTabPosition(QTabWidget.South)
        self.tabWidget.setDocumentMode(True)
        self.tab = QWidget()
        self.tab.setObjectName(u"tab")
        self.gridLayout = QGridLayout(self.tab)
        self.gridLayout.setObjectName(u"gridLayout")
        self.gridLayout.setContentsMargins(0, 0, 0, 0)
        self.objectTreeWidget = QTreeWidget(self.tab)
        __qtreewidgetitem = QTreeWidgetItem(self.objectTreeWidget)
        __qtreewidgetitem.setFlags(Qt.ItemIsSelectable|Qt.ItemIsUserCheckable|Qt.ItemIsEnabled|Qt.ItemIsAutoTristate)
        QTreeWidgetItem(__qtreewidgetitem)
        __qtreewidgetitem1 = QTreeWidgetItem(__qtreewidgetitem)
        QTreeWidgetItem(__qtreewidgetitem1)
        QTreeWidgetItem(__qtreewidgetitem1)
        QTreeWidgetItem(__qtreewidgetitem)
        self.objectTreeWidget.setObjectName(u"objectTreeWidget")
        self.objectTreeWidget.setAlternatingRowColors(True)
        self.objectTreeWidget.setSelectionMode(QAbstractItemView.NoSelection)
        self.objectTreeWidget.header().setStretchLastSection(False)

        self.gridLayout.addWidget(self.objectTreeWidget, 0, 0, 1, 1)

        self.tabWidget.addTab(self.tab, "")
        self.tab_2 = QWidget()
        self.tab_2.setObjectName(u"tab_2")
        self.gridLayout_2 = QGridLayout(self.tab_2)
        self.gridLayout_2.setObjectName(u"gridLayout_2")
        self.gridLayout_2.setContentsMargins(0, 0, 0, 0)
        self.permutationTreeWidget = QTreeWidget(self.tab_2)
        __qtreewidgetitem2 = QTreeWidgetItem(self.permutationTreeWidget)
        __qtreewidgetitem2.setFlags(Qt.ItemIsSelectable|Qt.ItemIsUserCheckable|Qt.ItemIsEnabled|Qt.ItemIsAutoTristate)
        QTreeWidgetItem(__qtreewidgetitem2)
        __qtreewidgetitem3 = QTreeWidgetItem(__qtreewidgetitem2)
        QTreeWidgetItem(__qtreewidgetitem3)
        QTreeWidgetItem(__qtreewidgetitem3)
        QTreeWidgetItem(__qtreewidgetitem2)
        self.permutationTreeWidget.setObjectName(u"permutationTreeWidget")
        self.permutationTreeWidget.setAlternatingRowColors(True)
        self.permutationTreeWidget.setSelectionMode(QAbstractItemView.NoSelection)
        self.permutationTreeWidget.header().setStretchLastSection(False)

        self.gridLayout_2.addWidget(self.permutationTreeWidget, 0, 0, 1, 1)

        self.tabWidget.addTab(self.tab_2, "")

        self.verticalLayout_3.addWidget(self.tabWidget)


        self.gridLayout_4.addWidget(self.groupBox_filter, 0, 0, 5, 1)

        self.groupBox_materialOptions = QGroupBox(Form)
        self.groupBox_materialOptions.setObjectName(u"groupBox_materialOptions")
        sizePolicy1 = QSizePolicy(QSizePolicy.Policy.Ignored, QSizePolicy.Policy.Preferred)
        sizePolicy1.setHorizontalStretch(0)
        sizePolicy1.setVerticalStretch(0)
        sizePolicy1.setHeightForWidth(self.groupBox_materialOptions.sizePolicy().hasHeightForWidth())
        self.groupBox_materialOptions.setSizePolicy(sizePolicy1)
        self.gridLayout_3 = QGridLayout(self.groupBox_materialOptions)
        self.gridLayout_3.setSpacing(8)
        self.gridLayout_3.setObjectName(u"gridLayout_3")
        self.label = QLabel(self.groupBox_materialOptions)
        self.label.setObjectName(u"label")

        self.gridLayout_3.addWidget(self.label, 0, 0, 1, 3)

        self.lineEdit_bitmapsFolder = QLineEdit(self.groupBox_materialOptions)
        self.lineEdit_bitmapsFolder.setObjectName(u"lineEdit_bitmapsFolder")
        self.lineEdit_bitmapsFolder.setMaxLength(512)

        self.gridLayout_3.addWidget(self.lineEdit_bitmapsFolder, 1, 0, 1, 2)

        self.label_2 = QLabel(self.groupBox_materialOptions)
        self.label_2.setObjectName(u"label_2")

        self.gridLayout_3.addWidget(self.label_2, 3, 0, 1, 1)

        self.toolButton_bitmapsFolder = QToolButton(self.groupBox_materialOptions)
        self.toolButton_bitmapsFolder.setObjectName(u"toolButton_bitmapsFolder")

        self.gridLayout_3.addWidget(self.toolButton_bitmapsFolder, 1, 2, 1, 1)

        self.lineEdit_bitmapsExtension = QLineEdit(self.groupBox_materialOptions)
        self.lineEdit_bitmapsExtension.setObjectName(u"lineEdit_bitmapsExtension")
        self.lineEdit_bitmapsExtension.setMaxLength(5)

        self.gridLayout_3.addWidget(self.lineEdit_bitmapsExtension, 3, 1, 1, 2)


        self.gridLayout_4.addWidget(self.groupBox_materialOptions, 2, 1, 1, 1)

        self.groupBox_meshOptions = QGroupBox(Form)
        self.groupBox_meshOptions.setObjectName(u"groupBox_meshOptions")
        self.verticalLayout_2 = QVBoxLayout(self.groupBox_meshOptions)
        self.verticalLayout_2.setSpacing(8)
        self.verticalLayout_2.setObjectName(u"verticalLayout_2")
        self.checkBox_splitMeshes = QCheckBox(self.groupBox_meshOptions)
        self.checkBox_splitMeshes.setObjectName(u"checkBox_splitMeshes")

        self.verticalLayout_2.addWidget(self.checkBox_splitMeshes)

        self.checkBox_importNormals = QCheckBox(self.groupBox_meshOptions)
        self.checkBox_importNormals.setObjectName(u"checkBox_importNormals")
        self.checkBox_importNormals.setChecked(True)

        self.verticalLayout_2.addWidget(self.checkBox_importNormals)

        self.checkBox_importWeights = QCheckBox(self.groupBox_meshOptions)
        self.checkBox_importWeights.setObjectName(u"checkBox_importWeights")
        self.checkBox_importWeights.setChecked(True)

        self.verticalLayout_2.addWidget(self.checkBox_importWeights)


        self.gridLayout_4.addWidget(self.groupBox_meshOptions, 1, 1, 1, 1)

        self.verticalSpacer = QSpacerItem(20, 0, QSizePolicy.Policy.Minimum, QSizePolicy.Policy.MinimumExpanding)

        self.gridLayout_4.addItem(self.verticalSpacer, 4, 1, 1, 1)

        QWidget.setTabOrder(self.checkBox_importBones, self.checkBox_importMarkers)
        QWidget.setTabOrder(self.checkBox_importMarkers, self.checkBox_importMeshes)
        QWidget.setTabOrder(self.checkBox_importMeshes, self.checkBox_importMaterials)
        QWidget.setTabOrder(self.checkBox_importMaterials, self.checkBox_splitMeshes)
        QWidget.setTabOrder(self.checkBox_splitMeshes, self.tabWidget)
        QWidget.setTabOrder(self.tabWidget, self.checkBox_importNormals)
        QWidget.setTabOrder(self.checkBox_importNormals, self.spinBox_objectScale)
        QWidget.setTabOrder(self.spinBox_objectScale, self.spinBox_boneScale)
        QWidget.setTabOrder(self.spinBox_boneScale, self.spinBox_markerScale)
        QWidget.setTabOrder(self.spinBox_markerScale, self.toolButton_expandAll)
        QWidget.setTabOrder(self.toolButton_expandAll, self.toolButton_collapseAll)
        QWidget.setTabOrder(self.toolButton_collapseAll, self.toolButton_checkAll)
        QWidget.setTabOrder(self.toolButton_checkAll, self.toolButton_uncheckAll)
        QWidget.setTabOrder(self.toolButton_uncheckAll, self.objectTreeWidget)
        QWidget.setTabOrder(self.objectTreeWidget, self.permutationTreeWidget)
        QWidget.setTabOrder(self.permutationTreeWidget, self.checkBox_importWeights)

        self.retranslateUi(Form)
        self.checkBox_importMeshes.toggled.connect(self.groupBox_meshOptions.setEnabled)
        self.checkBox_importBones.toggled.connect(self.checkBox_importWeights.setEnabled)
        self.checkBox_importMaterials.toggled.connect(self.groupBox_materialOptions.setEnabled)

        self.tabWidget.setCurrentIndex(0)


        QMetaObject.connectSlotsByName(Form)
    # setupUi

    def retranslateUi(self, Form):
        self.groupBox_importOptions.setTitle(QCoreApplication.translate("Form", u"Import Options", None))
        self.checkBox_importBones.setText(QCoreApplication.translate("Form", u"Import Bones", None))
        self.checkBox_importMarkers.setText(QCoreApplication.translate("Form", u"Import Markers", None))
        self.checkBox_importMeshes.setText(QCoreApplication.translate("Form", u"Import Meshes", None))
        self.checkBox_importMaterials.setText(QCoreApplication.translate("Form", u"Import Materials", None))
        self.groupBox_scaleOptions.setTitle(QCoreApplication.translate("Form", u"Scale Options", None))
        self.label_boneScale.setText(QCoreApplication.translate("Form", u"Bone Scale", None))
        self.label_markerScale.setText(QCoreApplication.translate("Form", u"Marker Scale", None))
        self.label_objectScale.setText(QCoreApplication.translate("Form", u"Object Scale", None))
        self.groupBox_filter.setTitle(QCoreApplication.translate("Form", u"Import Filter", None))
#if QT_CONFIG(tooltip)
        self.toolButton_expandAll.setToolTip(QCoreApplication.translate("Form", u"Expand All", None))
#endif // QT_CONFIG(tooltip)
#if QT_CONFIG(tooltip)
        self.toolButton_collapseAll.setToolTip(QCoreApplication.translate("Form", u"Collapse All", None))
#endif // QT_CONFIG(tooltip)
#if QT_CONFIG(tooltip)
        self.toolButton_checkAll.setToolTip(QCoreApplication.translate("Form", u"Check All", None))
#endif // QT_CONFIG(tooltip)
#if QT_CONFIG(tooltip)
        self.toolButton_uncheckAll.setToolTip(QCoreApplication.translate("Form", u"Uncheck All", None))
#endif // QT_CONFIG(tooltip)
        ___qtreewidgetitem = self.objectTreeWidget.headerItem()
        ___qtreewidgetitem.setText(1, QCoreApplication.translate("Form", u"Type", None))
        ___qtreewidgetitem.setText(0, QCoreApplication.translate("Form", u"Name", None))

        __sortingEnabled = self.objectTreeWidget.isSortingEnabled()
        self.objectTreeWidget.setSortingEnabled(False)
        ___qtreewidgetitem1 = self.objectTreeWidget.topLevelItem(0)
        ___qtreewidgetitem1.setText(1, QCoreApplication.translate("Form", u"Scene Item", None))
        ___qtreewidgetitem1.setText(0, QCoreApplication.translate("Form", u"Scene Item 1", None))
        ___qtreewidgetitem2 = ___qtreewidgetitem1.child(0)
        ___qtreewidgetitem2.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem3 = ___qtreewidgetitem1.child(1)
        ___qtreewidgetitem3.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem4 = ___qtreewidgetitem3.child(0)
        ___qtreewidgetitem4.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem5 = ___qtreewidgetitem3.child(1)
        ___qtreewidgetitem5.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem6 = ___qtreewidgetitem1.child(2)
        ___qtreewidgetitem6.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        self.objectTreeWidget.setSortingEnabled(__sortingEnabled)

        self.tabWidget.setTabText(self.tabWidget.indexOf(self.tab), QCoreApplication.translate("Form", u"Region View", None))
        ___qtreewidgetitem7 = self.permutationTreeWidget.headerItem()
        ___qtreewidgetitem7.setText(1, QCoreApplication.translate("Form", u"Type", None))
        ___qtreewidgetitem7.setText(0, QCoreApplication.translate("Form", u"Name", None))

        __sortingEnabled1 = self.permutationTreeWidget.isSortingEnabled()
        self.permutationTreeWidget.setSortingEnabled(False)
        ___qtreewidgetitem8 = self.permutationTreeWidget.topLevelItem(0)
        ___qtreewidgetitem8.setText(1, QCoreApplication.translate("Form", u"Scene Item", None))
        ___qtreewidgetitem8.setText(0, QCoreApplication.translate("Form", u"Scene Item 1", None))
        ___qtreewidgetitem9 = ___qtreewidgetitem8.child(0)
        ___qtreewidgetitem9.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem10 = ___qtreewidgetitem8.child(1)
        ___qtreewidgetitem10.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem11 = ___qtreewidgetitem10.child(0)
        ___qtreewidgetitem11.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem12 = ___qtreewidgetitem10.child(1)
        ___qtreewidgetitem12.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        ___qtreewidgetitem13 = ___qtreewidgetitem8.child(2)
        ___qtreewidgetitem13.setText(0, QCoreApplication.translate("Form", u"New Subitem", None))
        self.permutationTreeWidget.setSortingEnabled(__sortingEnabled1)

        self.tabWidget.setTabText(self.tabWidget.indexOf(self.tab_2), QCoreApplication.translate("Form", u"Permutation View", None))
        self.groupBox_materialOptions.setTitle(QCoreApplication.translate("Form", u"Material Options", None))
        self.label.setText(QCoreApplication.translate("Form", u"Bitmaps Folder", None))
        self.label_2.setText(QCoreApplication.translate("Form", u"File Extension", None))
        self.toolButton_bitmapsFolder.setText(QCoreApplication.translate("Form", u"...", None))
        self.groupBox_meshOptions.setTitle(QCoreApplication.translate("Form", u"Mesh Options", None))
        self.checkBox_splitMeshes.setText(QCoreApplication.translate("Form", u"Split By Material", None))
        self.checkBox_importNormals.setText(QCoreApplication.translate("Form", u"Import Vertex Normals", None))
        self.checkBox_importWeights.setText(QCoreApplication.translate("Form", u"Import Vertex Weights", None))
        pass
    # retranslateUi


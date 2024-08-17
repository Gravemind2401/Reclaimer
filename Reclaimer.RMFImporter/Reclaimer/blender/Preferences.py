from bpy.types import Context, AddonPreferences
from bpy.props import BoolProperty, StringProperty, FloatProperty, FloatVectorProperty

from ..src.ImportOptions import ImportOptions
from .DependencyInstallerOperator import DependencyInstallerOperator

_first_draw = True
_was_missing_dependencies = False


class RmfPreferences(AddonPreferences):
    bl_idname = '.'.join(__package__.split('.')[:-1])

    import_bones: BoolProperty(
        name = 'Import Bones',
        description = 'Determines if an armature and bones will be created where applicable',
        default = ImportOptions.IMPORT_BONES
    ) # type: ignore

    import_markers: BoolProperty(
        name = 'Import Markers',
        description = 'Determines if markers will be created where applicable',
        default = ImportOptions.IMPORT_MARKERS
    ) # type: ignore

    import_meshes: BoolProperty(
        name = 'Import Meshes',
        description = 'Determines if mesh geometry will be created',
        default = ImportOptions.IMPORT_MESHES
    ) # type: ignore

    import_materials: BoolProperty(
        name = 'Import Materials',
        description = 'Determines if materials will be created and applied',
        default = ImportOptions.IMPORT_MATERIALS
    ) # type: ignore

    import_custom_props: BoolProperty(
        name = 'Import Custom Properties',
        description = 'Determines if custom properties will be applied to the imported objects',
        default = ImportOptions.IMPORT_CUSTOM_PROPS
    ) # type: ignore

    split_meshes: BoolProperty(
        name = 'Split By Material',
        description = 'Determines if meshes will be split by material',
        default = ImportOptions.SPLIT_MESHES
    ) # type: ignore

    import_normals: BoolProperty(
        name = 'Import Vertex Normals',
        description = 'Determines if vertex normals will be imported',
        default = ImportOptions.IMPORT_NORMALS
    ) # type: ignore

    import_skin: BoolProperty(
        name = 'Import Vertex Weights',
        description = 'Determines vertex weights will be imported',
        default = ImportOptions.IMPORT_SKIN
    ) # type: ignore

    object_scale: FloatProperty(
        name = 'Object Scale',
        description = 'Sets the size of imported meshes',
        default = ImportOptions.OBJECT_SCALE,
        min = 0.01
    ) # type: ignore

    bone_scale: FloatProperty(
        name = 'Bone Scale',
        description = 'Sets the size of imported bones',
        default = ImportOptions.BONE_SCALE,
        min = 0.01
    ) # type: ignore

    marker_scale: FloatProperty(
        name = 'Marker Scale',
        description = 'Sets the size of imported markers',
        default = ImportOptions.MARKER_SCALE,
        min = 0.01
    ) # type: ignore

    bone_prefix: StringProperty(
        name = 'Bone Name Prefix',
        description = 'The prefix to apply to all bone names',
        default = ImportOptions.BONE_PREFIX
    ) # type: ignore

    marker_prefix: StringProperty(
        name = 'Marker Name Prefix',
        description = 'The prefix to apply to all marker names',
        default = ImportOptions.MARKER_PREFIX
    ) # type: ignore

    bitmap_root: StringProperty(
        name = 'Bitmap Folder',
        description = 'The root folder where bitmaps are saved',
        default = ImportOptions.BITMAP_ROOT,
        subtype = 'DIR_PATH'
    ) # type: ignore

    bitmap_ext: StringProperty(
        name = 'Bitmap Extension',
        description = 'The file extension of the source bitmap files',
        default = ImportOptions.BITMAP_EXT
    ) # type: ignore

    cc_1: FloatVectorProperty(
        name = 'Default Primary Color',
        description = 'Initial primary color for color-change shader nodes',
        default = ImportOptions.DEFAULTCC_1,
        subtype = 'COLOR',
        size = 4,
        min = 0,
        max = 1
    ) # type: ignore

    cc_2: FloatVectorProperty(
        name = 'Default Secondary Color',
        description = 'Initial secondary color for color-change shader nodes',
        default = ImportOptions.DEFAULTCC_2,
        subtype = 'COLOR',
        size = 4,
        min = 0,
        max = 1
    ) # type: ignore

    cc_3: FloatVectorProperty(
        name = 'Default Tertiary Color',
        description = 'Initial tertiary color for color-change shader nodes',
        default = ImportOptions.DEFAULTCC_3,
        subtype = 'COLOR',
        size = 4,
        min = 0,
        max = 1
    ) # type: ignore

    def draw(self, context: Context):
        global _first_draw, _was_missing_dependencies

        layout = self.layout

        if _first_draw:
            _was_missing_dependencies = DependencyInstallerOperator.poll(context)
            _first_draw = False

        if _was_missing_dependencies:
            box = layout.box()
            box.row().label(icon='ERROR', text='You may need to run Blender as administrator to complete installation!')
            box.row().operator(DependencyInstallerOperator.bl_idname, icon='IMPORT')
            box.row().label(icon='ERROR', text='You must restart Blender after dependencies have been installed!')
            return

        layout.box().row().label(icon='CHECKMARK', text='Dependencies are installed')

        panel = layout.box()
        panel.label(icon='TOOL_SETTINGS', text='Default Values')
        panel.label(text='The default values to use each time the import dialog opens')

        box = panel.box()
        box.label(icon='IMPORT', text='Import Options')
        box.prop(self, 'import_bones')
        box.prop(self, 'import_markers')
        box.prop(self, 'import_meshes')
        box.prop(self, 'import_materials')
        box.prop(self, 'import_custom_props')

        box = panel.box()
        box.label(icon='MESH_DATA', text='Mesh Options')
        box.prop(self, 'split_meshes')
        box.prop(self, 'import_normals')
        box.prop(self, 'import_skin')

        box = panel.box()
        box.label(icon='COPY_ID', text='Naming Options')
        box.prop(self, 'bone_prefix')
        box.prop(self, 'marker_prefix')

        box = panel.box()
        box.label(icon='MATERIAL_DATA', text='Material Options')
        box.prop(self, 'bitmap_root')
        box.prop(self, 'bitmap_ext')

        box = panel.box()
        box.label(icon='WORLD_DATA', text='Scale Options')
        box.prop(self, 'object_scale')
        box.prop(self, 'bone_scale')
        box.prop(self, 'marker_scale')

        box = panel.box()
        box.label(icon='COLOR', text='Color Options')
        box.prop(self, 'cc_1')
        box.prop(self, 'cc_2')
        box.prop(self, 'cc_3')

import bpy
import bmesh
import bpy_extras
from typing import Set
from bpy.types import Context, Operator
from bpy.props import BoolProperty, FloatProperty, StringProperty, EnumProperty

from ..src.SceneReader import SceneReader

class RmfImportOperator(Operator, bpy_extras.io_utils.ImportHelper):
    '''Import an RMF file'''
    bl_idname: str = 'import_scene.rmf'
    bl_label: str = 'Import RMF'
    bl_options: Set[str] = {'PRESET', 'UNDO'}

    filename_ext: str = '.rmf'
    filter_glob: StringProperty(
        default = '*.rmf',
        options = {'HIDDEN'},
    )

    import_units: EnumProperty(
        name = 'Units',
        items = [
            ('METERS', 'Meters', 'Import using meters'),
            ('HALO', 'Halo', 'Import using Halo\'s world units'),
            ('MAX', '3DS Max', 'Import using 3DS Max units (100x Halo)'),
        ]
    )

    import_scale: FloatProperty(
        name = 'Multiplier',
        description = 'Sets the size of the imported model',
        default = 1.0,
        min = 0.0
    )

    import_nodes: BoolProperty(
        name = 'Import Bones',
        description = 'Determines if an armature and bones will be created, when applicable',
        default = True
    )

    node_prefix: StringProperty(
        name = 'Prefix',
        description = 'Adds a custom prefix in front of bone names',
        default = ''
    )

    import_markers: BoolProperty(
        name = 'Import Markers',
        description = 'Determines if marker spheres will be created, when applicable',
        default = True
    )

    marker_mode: EnumProperty(
        name = 'Type',
        items = [
            ('EMPTY', 'Empty Sphere', 'Create markers using empty spheres'),
            ('MESH', 'Mesh Sphere', 'Create markers using UVSpheres'),
        ]
    )

    marker_prefix: StringProperty(
        name = 'Prefix',
        description = 'Adds a custom prefix in front of marker names',
        default = '#'
    )

    import_meshes: BoolProperty(
        name = 'Import Meshes',
        description = 'Determines if mesh geometry will be created',
        default = True
    )

    mesh_mode: EnumProperty(
        name = 'Group by',
        items = [
            ('JOIN', 'Permutation', 'Create one mesh per permutation'),
            ('SPLIT', 'Material', 'Create one mesh per material index'),
        ]
    )

    import_materials: BoolProperty(
        name = 'Import Materials',
        description = 'Determines if materials will be created and applied',
        default = True
    )

    bitmap_dir: StringProperty(
        name = 'Folder',
        description = 'The root folder where bitmaps are saved',
        default = '',
        #subtype = 'DIR_PATH' # blender will not allow us to open a browser dilaog while the import dialog is open
    )

    bitmap_ext: StringProperty(
        name = 'Extension',
        description = 'The file extension of the source bitmap files',
        default = 'tif'
    )

    def execute(self, context: Context) -> Set[str]:
        #options = ImportOptions()
        #options.IMPORT_SCALE = self.import_scale
        #options.IMPORT_BONES = self.import_nodes
        #options.IMPORT_MARKERS = self.import_markers
        #options.IMPORT_MESHES = self.import_meshes
        #options.IMPORT_MATERIALS = self.import_materials

        #options.PREFIX_MARKER = self.marker_prefix
        #options.PREFIX_BONE = self.node_prefix

        #options.DIRECTORY_BITMAP = self.bitmap_dir
        #options.SUFFIX_BITMAP = self.bitmap_ext.lstrip(' .').rstrip()

        #options.MODE_SCALE = self.import_units
        #options.MODE_MARKERS = self.marker_mode
        #options.MODE_MESHES = self.mesh_mode

        #main(context, self.filepath, options)
        scene = SceneReader.open_scene(self.filepath)
        print(f'scene name: {scene.name}')

        return {'FINISHED'}

    def draw(self, context: Context):
        layout = self.layout

        box = layout.box()
        box.label(icon = 'WORLD_DATA', text = 'Scale')
        box.prop(self, 'import_units')
        box.prop(self, 'import_scale')

        box = layout.box()
        row = box.row()
        row.label(icon = 'BONE_DATA', text = 'Bones')
        row.prop(self, 'import_nodes')
        if self.import_nodes:
            box.prop(self, 'node_prefix')

        box = layout.box()
        row = box.row()
        row.label(icon = 'MESH_CIRCLE', text = 'Markers')
        row.prop(self, 'import_markers')
        if self.import_markers:
            box.prop(self, 'marker_mode')
            box.prop(self, 'marker_prefix')

        box = layout.box()
        row = box.row()
        row.label(icon = 'MATERIAL_DATA', text = 'Materials')
        row.prop(self, 'import_materials')
        if self.import_materials:
            box.prop(self, 'bitmap_dir')
            box.prop(self, 'bitmap_ext')

        box = layout.box()
        row = box.row()
        row.label(icon = 'MESH_DATA', text = 'Meshes')
        row.prop(self, 'import_meshes')
        if self.import_meshes:
            box.prop(self, 'mesh_mode')
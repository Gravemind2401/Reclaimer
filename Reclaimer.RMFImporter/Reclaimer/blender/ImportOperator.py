import bpy
import bpy_extras

from typing import cast, Set
from bpy.types import Context, Operator
from bpy.props import StringProperty, BoolProperty

from . import RmfPreferences
from ..src.ImportOptions import ImportOptions
from ..src.SceneReader import SceneReader
from ..src.SceneFilter import *


class RmfImportOperator(Operator, bpy_extras.io_utils.ImportHelper):
    '''Import an RMF file'''

    bl_idname: str = 'rmf.import_operator'
    bl_label: str = 'Import RMF'

    filter_glob: StringProperty(
        default = '*.rmf',
        options = {'HIDDEN'},
    ) # type: ignore

    nogui: BoolProperty(
        default = False
    ) # type: ignore

    def execute(self, context: Context) -> Set[str]:
        # this cast is just for autocomplete and intellisense
        preferences = cast(RmfPreferences, context.preferences.addons[RmfPreferences.bl_idname].preferences)

        ImportOptions.IMPORT_BONES = preferences.import_bones
        ImportOptions.IMPORT_MARKERS = preferences.import_markers
        ImportOptions.IMPORT_MESHES = preferences.import_meshes
        ImportOptions.IMPORT_MATERIALS = preferences.import_materials

        ImportOptions.IMPORT_CUSTOM_PROPS = preferences.import_custom_props

        ImportOptions.SPLIT_MESHES = preferences.split_meshes
        ImportOptions.IMPORT_NORMALS = preferences.import_normals
        ImportOptions.IMPORT_SKIN = preferences.import_skin
        # ImportOptions.IMPORT_UVW = preferences.import_uvw
        # ImportOptions.IMPORT_COLORS = preferences.import_colors

        ImportOptions.OBJECT_SCALE = preferences.object_scale
        ImportOptions.BONE_SCALE = preferences.bone_scale
        ImportOptions.MARKER_SCALE = preferences.marker_scale

        ImportOptions.BONE_PREFIX = preferences.bone_prefix
        ImportOptions.MARKER_PREFIX = preferences.marker_prefix

        ImportOptions.BITMAP_ROOT = preferences.bitmap_root
        ImportOptions.BITMAP_EXT = preferences.bitmap_ext

        ImportOptions.DEFAULTCC_1 = preferences.cc_1
        ImportOptions.DEFAULTCC_2 = preferences.cc_2
        ImportOptions.DEFAULTCC_3 = preferences.cc_3
        # ImportOptions.DEFAULTCC_4 = preferences.cc_4

        if preferences.nogui or self.nogui:
            self._import_nogui()
        else:
            bpy.ops.rmf.dialog_operator('EXEC_DEFAULT', filepath=self.filepath)

        return {'FINISHED'}

    def _import_nogui(self):
        try:
            scene = SceneReader.open_scene(self.filepath)
        except Exception as e:
            self.report({'ERROR'}, 'An error occured during the import. See the console window for details.')
            print('\n===============\nERROR DETAILS\n===============\n')
            raise e

        filter = SceneFilter(scene)
        options = ImportOptions(scene)

        bpy.types.Scene.rmf_data = {
            'scene': scene,
            'filter': filter,
            'options': options
        }

        bpy.ops.rmf.progress_operator('EXEC_DEFAULT')
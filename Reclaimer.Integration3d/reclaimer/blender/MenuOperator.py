import bpy
from typing import Set
from bpy.types import Context, Operator

class IMPORT_SCENE_MT_rmf(Operator):
    '''Import an RMF file'''
    bl_idname = 'menu_import.rmf'
    bl_label = 'RMF (.rmf)'

    def execute(self, context: Context) -> Set[str]:
        bpy.ops.import_scene.rmf('INVOKE_DEFAULT')
        return {'FINISHED'}
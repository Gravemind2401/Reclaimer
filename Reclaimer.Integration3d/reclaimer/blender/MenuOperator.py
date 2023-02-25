import bpy
from typing import Set
from bpy.types import Context, Operator

class RmfMenuOperator(Operator):
    '''Import an RMF file'''
    bl_idname: str = 'menu_import.rmf'
    bl_label: str = 'RMF (.rmf)'

    def execute(self, context: Context) -> Set[str]:
        bpy.ops.import_scene.rmf('INVOKE_DEFAULT')
        return {'FINISHED'}
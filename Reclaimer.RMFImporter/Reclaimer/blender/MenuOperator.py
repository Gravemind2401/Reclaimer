import bpy
from typing import Set
from bpy.types import Context, Operator


class RmfMenuOperator(Operator):
    '''Import an RMF file'''

    bl_idname: str = 'rmf.menu_operator'
    bl_label: str = 'RMF (.rmf)'

    def execute(self, context: Context) -> Set[str]:
        bpy.ops.rmf.import_operator('INVOKE_DEFAULT')
        return {'FINISHED'}

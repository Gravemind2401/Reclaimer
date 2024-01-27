from bpy.types import Context, AddonPreferences
from .DependencyInstallerOperator import DependencyInstallerOperator

_first_draw = True
_was_missing_dependencies = False


class RmfPreferences(AddonPreferences):
    bl_idname = __package__.split('.')[0]

    def draw(self, context: Context):
        global _first_draw, _was_missing_dependencies

        layout = self.layout

        if _first_draw:
            _was_missing_dependencies = DependencyInstallerOperator.poll(context)
            _first_draw = False
        
        if _was_missing_dependencies:
            layout.label(icon='ERROR', text='You may need to run Blender as administrator to complete installation!')
            layout.operator(DependencyInstallerOperator.bl_idname, icon='IMPORT')
            layout.label(icon='ERROR', text='You must restart Blender after dependencies have been installed!')
        else:
            layout.label(icon='CHECKMARK', text='Dependencies are installed')

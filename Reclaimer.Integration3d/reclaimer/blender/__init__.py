import bpy
from bpy.types import Context, Operator
from .Preferences import RmfPreferences
from .DependencyInstallerOperator import *

# preferences always get registered
__preferences__ = [
    DependencyInstallerOperator,
    RmfPreferences
]

# operators only get registered if dependencies are installed
__operators__ = []

if try_import_dependencies():
    from .ImportOperator import RmfImportOperator
    from .MenuOperator import RmfMenuOperator
    from .DialogOperator import RmfDialogOperator

    __operators__ = [
        RmfDialogOperator,
        RmfImportOperator,
        RmfMenuOperator
    ]


def _draw_menu_operator(operator: Operator, context: Context):
    operator.layout.operator(RmfMenuOperator.bl_idname)

def clean_scene(check_users: bool = False):
    for item in bpy.data.objects:
        if item.type == 'MESH' or item.type == 'EMPTY':
            bpy.data.objects.remove(item)

    for collection in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials, bpy.data.textures, bpy.data.images):
        for item in collection:
            if item.users == 0 or not check_users:
                collection.remove(item)

def register():
    """ Blender Addon Entry Point """

    for cls in __preferences__:
        bpy.utils.register_class(cls)

    if not __operators__:
        return # operators not imported

    for cls in __operators__:
        bpy.utils.register_class(cls)

    bpy.types.TOPBAR_MT_file_import.append(_draw_menu_operator)

def unregister():
    """ Blender Addon Entry Point """

    for cls in reversed(__preferences__):
        bpy.utils.unregister_class(cls)

    if not __operators__:
        return # operators were never registered

    bpy.types.TOPBAR_MT_file_import.remove(_draw_menu_operator)

    for cls in reversed(__operators__):
        bpy.utils.unregister_class(cls)

def reset():
    try:
        unregister()
    finally:
        clean_scene()
        register()
        bpy.ops.import_scene.rmf('INVOKE_DEFAULT')
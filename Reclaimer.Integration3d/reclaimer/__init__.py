"""
Reclaimer RMF Import Utility for Blender
"""

bl_info = {
    'name': 'RMF Importer',
    'description': 'Import RMF files created by Reclaimer.',
    'author': 'Gravemind2401',
    'version': (1, 0, 0),
    'blender': (2, 80, 0),
    'location': 'File > Import > RMF',
    'category': 'Import-Export',
    'support': 'TESTING'
}

if 'bpy' in locals():
    from importlib import reload
    if 'IMPORT_SCENE_OT_rmf' in locals():
        reload(IMPORT_SCENE_OT_rmf)
    if 'IMPORT_SCENE_MT_rmf' in locals():
        reload(IMPORT_SCENE_MT_rmf)
else:
    import bpy
    from bpy.types import Context, Operator
    from .blender.ImportHelper import IMPORT_SCENE_OT_rmf
    from .blender.MenuOperator import IMPORT_SCENE_MT_rmf

def _draw_menu_operator(operator: Operator, context: Context):
    operator.layout.operator(IMPORT_SCENE_MT_rmf.bl_idname)
    
def _clean_scene():
    for item in bpy.data.objects:
        if item.type == 'MESH' or item.type == 'EMPTY':
            bpy.data.objects.remove(item)

    check_users = False
    for collection in (bpy.data.meshes, bpy.data.armatures, bpy.data.materials, bpy.data.textures, bpy.data.images):
        for item in collection:
            if item.users == 0 or not check_users:
                collection.remove(item)

def register():
    """ Blender Addon Entry Point """
    bpy.utils.register_class(IMPORT_SCENE_OT_rmf)
    bpy.utils.register_class(IMPORT_SCENE_MT_rmf)
    bpy.types.TOPBAR_MT_file_import.append(_draw_menu_operator)

def unregister():
    """ Blender Addon Entry Point """
    bpy.types.TOPBAR_MT_file_import.remove(_draw_menu_operator)
    bpy.utils.unregister_class(IMPORT_SCENE_MT_rmf)
    bpy.utils.unregister_class(IMPORT_SCENE_OT_rmf)


if __name__ == '__main__':
    try:
        unregister()
    finally:
        _clean_scene()
        register()
        bpy.ops.import_scene.rmf('INVOKE_DEFAULT')
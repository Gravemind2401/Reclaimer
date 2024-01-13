"""
Reclaimer RMF Import Utility
"""

from importlib import util as importutil

# blender appears to parse this directly from the python file
# as text so it cannot be imported from within a submodule
bl_info = {
    'name': 'RMF Importer',
    'description': 'Import RMF files created by Reclaimer.',
    'author': 'Gravemind2401',
    'version': (1, 0, 0),
    'blender': (2, 91, 0),
    'location': 'File > Import > RMF',
    'warning': 'Requires installation of PySide2',
    'category': 'Import-Export',
    'support': 'TESTING'
}

if importutil.find_spec('bpy'):
    # these imports are unused locally, but need to be
    # imported so blender can find them
    from . import blender
    from .blender import register, unregister
    from .blender import reset as import_rmf

if importutil.find_spec('pymxs'):
    from . import autodesk
    from .autodesk import import_rmf
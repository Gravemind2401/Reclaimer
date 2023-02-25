"""
Reclaimer RMF Import Utility
"""

# blender appears to parse this directly from the python file
# as text so it cannot be imported from within a submodule
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

# these imports are unused locally, but need to be
# imported so blender can find them
from . import blender
from .blender import register, unregister

if __name__ == '__main__':
    blender.reset()
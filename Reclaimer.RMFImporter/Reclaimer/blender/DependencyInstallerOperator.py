import sys, sysconfig, subprocess
from collections import namedtuple
from typing import Set
from bpy.types import Context, Operator
from .DependencyUtils import *

__all__ = [
    'try_import_dependencies',
    'DependencyInstallerOperator'
]

# Blender 4.1+ use python 3.11 and later which is not explicitly supported by PySide2, however it still appears to work just fine if we bypass the version requirements
# (as a side note, PySide6 was attempted but it just crashed Blender entirely as soon the importer window attempted to open)
_pyside_pip_args = None if sys.version_info <= (3, 10) else ['--ignore-requires-python', '--python-version=310', '--only-binary=:all:', '--target', sysconfig.get_path("purelib")]

Dependency = namedtuple('Dependency', ['module', 'package', 'name', 'args'])

_dependencies = [Dependency(module='PySide2', package=None, name=None, args=_pyside_pip_args)]
_dependencies_installed = False


def try_import_dependencies() -> bool:
    global _dependencies_installed

    try:
        for module, package, name, args in _dependencies:
            import_module(module_name=module, global_name=name)
        _dependencies_installed = True
    except ModuleNotFoundError:
        pass

    return _dependencies_installed


class DependencyInstallerOperator(Operator):
    '''Downloads and installs the required python packages for this add-on.
Internet connection is required. Blender may have to be started with
elevated permissions in order to install the package'''

    bl_idname = 'rmf.dependency_installer'
    bl_label = 'Install Dependencies'
    bl_options = {'REGISTER', 'INTERNAL'}

    @classmethod
    def poll(self, context: Context) -> bool:
        # Deactivate when dependencies have been installed
        return not _dependencies_installed

    def execute(self, context: Context) -> Set[str]:
        try:
            install_pip()
            for dependency in _dependencies:
                install_and_import_module(module_name=dependency.module,
                                          package_name=dependency.package,
                                          global_name=dependency.name,
                                          install_args=dependency.args)
        except (subprocess.CalledProcessError, ImportError) as err:
            self.report({'ERROR'}, str(err))
            return {'CANCELLED'}

        global _dependencies_installed
        _dependencies_installed = True

        return {'FINISHED'}

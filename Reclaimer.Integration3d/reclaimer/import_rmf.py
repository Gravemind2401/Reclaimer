import os
import sys
import importlib
import importlib.machinery
from contextlib import contextmanager

# this file must be in the same directory as the root level __init__ file
PACKAGE_NAME = os.path.basename(os.path.dirname(__file__))

class TempFinder(importlib.machinery.PathFinder):
    _path = []

    @classmethod
    def find_spec(cls, fullname, path=None, target=None):
        return super().find_spec(fullname, cls._path, target)

@contextmanager
def finder():
    try:
        package_dir = os.path.dirname(__file__)
        TempFinder._path = [os.path.join(package_dir, '..')]
        print(f'adding temporary PathFinder for "{package_dir}"')
        sys.meta_path.append(TempFinder)
        yield
    finally:
        sys.meta_path.remove(TempFinder)
        print('removed temporary PathFinder')

if PACKAGE_NAME in sys.modules:
    print('reloading reclaimer module...')
    with finder():
        importlib.reload(sys.modules[PACKAGE_NAME])
else:
    print('importing reclaimer module...')
    with finder():
        importlib.import_module(PACKAGE_NAME)

getattr(sys.modules[PACKAGE_NAME], 'import_rmf')()
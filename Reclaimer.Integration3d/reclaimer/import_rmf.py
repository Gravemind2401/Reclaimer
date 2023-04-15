import os
import sys
import importlib
import importlib.machinery
from contextlib import contextmanager

# this file must be in the same directory as the root level __init__ file
# and the directory must be called 'reclaimer'
RECLAIMER = 'reclaimer'

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
    
if RECLAIMER in sys.modules:
    print('reloading reclaimer module...')
    with finder():
        importlib.reload(sys.modules[RECLAIMER])
else:
    print('importing reclaimer module...')
    with finder():
        importlib.import_module(RECLAIMER)

getattr(sys.modules[RECLAIMER], 'import_rmf')()
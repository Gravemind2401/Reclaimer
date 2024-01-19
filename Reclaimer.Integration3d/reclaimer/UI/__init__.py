import os, inspect

UI_ROOT = os.path.dirname(inspect.getabsfile(inspect.currentframe()))
WIDGET_UI_FILE = os.path.join(UI_ROOT, 'widget.ui')
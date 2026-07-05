Set-Alias uic ".\.venv\Scripts\pyside6-uic.exe"
Set-Alias rcc ".\.venv\Scripts\pyside6-rcc.exe"

uic .\Reclaimer\ui\progress.ui -o .\Reclaimer\ui\progress_ui.py --from-imports
uic .\Reclaimer\ui\widget.ui -o .\Reclaimer\ui\widget_ui.py --from-imports
rcc .\Reclaimer\ui\resources\resources.qrc -o .\Reclaimer\ui\resources_rc.py

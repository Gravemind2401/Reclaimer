macroScript ImportRMF
    category: "File"
    toolTip: "Import RMF"
(
    on execute do (
        local scriptsDir = GetDir #scripts
        local pyScript = scriptsDir + "\\Reclaimer\\import_rmf.py"
        python.ExecuteFile pyScript
    )
)
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [System.IO.FileInfo]$WheelFileInfo
)

process {
    $WheelPath = $WheelFileInfo.FullName
    $WheelName = $WheelFileInfo.Name

    if (-not $WheelFileInfo.Exists) {
        Write-Error "Could not find the wheel file at: $WheelPath"
        return # 'return' acts like 'continue' inside a script's process block
    }

    Write-Host "Repacking $WheelName..." -ForegroundColor Cyan

    $ParentDir = $WheelFileInfo.DirectoryName
    $TempExtract = Join-Path $ParentDir "$($WheelFile.BaseName)_temp"

    if (Test-Path $TempExtract) {
        Remove-Item -Recurse -Force $TempExtract
    }

    # Extract the .whl (whl files are just zips)
    # Need to rename first because Expand-Archive throws if the extension is not zip
    Write-Host "Extracting $WheelName..."

    $ZipFile = [IO.Path]::ChangeExtension($WheelPath, ".zip")
    Rename-Item -Path $WheelPath -NewName $ZipFile
    Expand-Archive -Path $ZipFile -DestinationPath $TempExtract

    # Locate and remove debug folders which are overflowing the max path limit
    $FoundProblems = $false
    $ProblemPaths = "PySide6\qml\Qt\labs\assetdownloader\objects-Debug", "PySide6\qml\Qt\labs\assetdownloader\objects-RelWithDebInfo"
    foreach ($ProblemPath in $ProblemPaths) {
        $ProblemPathFull = Join-Path $TempExtract $ProblemPath

        if (Test-Path $ProblemPathFull) {
            $FoundProblems = $true
            Write-Host "Removing directory: \$ProblemPath"
            Remove-Item -Recurse -Force $ProblemPathFull
        }
    }

    if (-not $FoundProblems) {
        Write-Host "Found no issues with $WheelName - cancelling repack" -ForegroundColor Cyan
        Rename-Item -Path $ZipFile -NewName $WheelPath
        Remove-Item -Recurse -Force $TempExtract
        return
    }

    # Repack the directory and overwrite the old .whl file
    Write-Host "Repacking archive..."
    Get-ChildItem -Path $TempExtract | Compress-Archive -DestinationPath $ZipFile -CompressionLevel Optimal -Force

    Rename-Item -Path $ZipFile -NewName $WheelPath
    Remove-Item -Recurse -Force $TempExtract

    Write-Host "Repack complete for $WheelName" -ForegroundColor Cyan
}

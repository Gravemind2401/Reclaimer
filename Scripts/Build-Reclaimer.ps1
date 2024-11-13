param (
    [string] $TargetBranch = $null
)

function Rasterize-ApplicationIcon {
    if (-not (Test-Path (Get-Alias inkscape).Definition)) {
        Write-Warning 'Could not rasterize application icon: inkscape not found!'
        return
    }

    if (-not (Test-Path (Get-Alias icomake).Definition)) {
        Write-Warning 'Could not rasterize application icon: icomake not found!'
        return
    }

    Write-Host 'Rasterizing application icon...' -ForegroundColor Yellow

    $outputSizes = 16, 24, 32, 48, 64, 96, 128, 256
    $outputNameArray = $outputSizes | ForEach-Object { ".\obj\Reclaimer_$($_).png" }

    foreach ($size in $outputSizes) {
        $inputFile = if ($size -le 48) { '.\obj\res\Reclaimer48.svg' } else { '.\obj\res\Reclaimer.svg' }
        
        #this is piped to Out-Null to make it wait for the inkscape process to finish before continuing, otherwise it might not be finished by the time icomake is called
        inkscape --export-filename=".\obj\Reclaimer_$size.png" --export-overwrite --export-width=$size --export-height=$size "$inputFile" | Out-Null
    }

    icomake ".\obj\Reclaimer.ico" $outputNameArray | Out-Null

    Copy-Item ".\obj\Reclaimer.ico" -Destination "$buildRoot\Reclaimer\Reclaimer\Resources\Reclaimer.ico" -Force
}

function Rasterize-InstallerResources {
    if (-not (Test-Path (Get-Alias inkscape).Definition)) {
        Write-Warning 'Could not rasterize installer resources: inkscape not found!'
        return
    }

    Write-Host 'Rasterizing installer resources...' -ForegroundColor Yellow

    foreach ($fileName in '.\obj\res\bannerbmp.svg', '.\obj\res\dlgbmp.svg') {
        [IO.File]::WriteAllText($fileName, ([IO.File]::ReadAllText($fileName) -replace '#VERSION#', $assemblyVersion))
    }

    inkscape --export-filename=".\obj\Banner.png" --export-width=493 --export-height=58 ".\obj\res\bannerbmp.svg" | Out-Null
    inkscape --export-filename=".\obj\Background.png" --export-width=493 --export-height=312 ".\obj\res\dlgbmp.svg" | Out-Null

    Copy-Item ".\obj\Banner.png", ".\obj\Background.png" -Destination "$buildRoot\Reclaimer\Reclaimer.Setup" -Force
}

function Build-Installer {

    # build Reclaimer on its own first, otherwise the DefineConstants override for the installer would also apply to all dependencies
    Write-Host 'Building Reclaimer...' -ForegroundColor Yellow
    dotnet build "$buildRoot\Reclaimer\Reclaimer\Reclaimer.csproj" -c Release --property:AssemblyVersion=$assemblyVersion

    Write-Host 'Building Installer...' -ForegroundColor Yellow
    dotnet build "$buildRoot\Reclaimer\Reclaimer.Setup\Reclaimer.Setup.wixproj" -c Release --property:AssemblyVersion=$assemblyVersion --property:DefineConstants="UseCustomImages" --no-dependencies

}

function Build-ImportScript {
    Set-Alias pip "C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64\Scripts\pip.exe"
    Set-Alias blender "C:\Program Files\Blender Foundation\Blender 4.2\blender.exe"

    if (-not (Test-Path (Get-Alias pip).Definition)) {
        Write-Warning 'Could not build RMF Importer: pip not found!'
        return
    }

    if (-not (Test-Path (Get-Alias blender).Definition)) {
        Write-Warning 'Could not build RMF Importer: blender not found!'
        return
    }

    Write-Host 'Building RMF Importer...' -ForegroundColor Yellow

    # copy RMF importer files
    Copy-Item "$buildRoot\Reclaimer\Reclaimer.RMFImporter\Reclaimer" ".\obj\RMFImporter\Reclaimer" -Recurse
    Copy-Item "$buildRoot\Reclaimer\Reclaimer.RMFImporter\README.md" ".\obj\RMFImporter\Reclaimer\README.txt"

    # cleanup python cache files
    Get-ChildItem ".\obj\RMFImporter\Reclaimer" -Directory -Recurse `
        | Where-Object { $_.Name -eq '__pycache__' -or $_.Name -eq 'tests' } `
        | Remove-Item -Recurse -Force

    # remove the blender manifest so it doesnt get included in the cross platform zip
    Remove-Item ".\obj\RMFImporter\Reclaimer\blender_manifest.toml"

    # create RMF Importer plugin zip (cross platform)
    # do this before wheels downloaded so it doesnt include them in the zip
    Compress-Archive ".\obj\RMFImporter\Reclaimer" ".\bin\reclaimer_rmf_importer-$scriptVersion-xplatform.zip" -CompressionLevel Optimal

    # restore the blender manifest now that we are building the 4.2+ zips
    Copy-Item "$buildRoot\Reclaimer\Reclaimer.RMFImporter\Reclaimer\blender_manifest.toml" ".\obj\RMFImporter\Reclaimer\"

    # prepare wheels for Blender 4.2
    # need to disable version check because it gets treated as an error
    pip download pyside2 --disable-pip-version-check --dest ".\obj\RMFImporter\Reclaimer\wheels" --only-binary=:all: --python-version 3.10 --platform win_amd64
    pip download pyside2 --disable-pip-version-check --dest ".\obj\RMFImporter\Reclaimer\wheels" --only-binary=:all: --python-version 3.10 --platform manylinux1_x86_64

    # build extension zips for Blender 4.2
    blender --command extension build --split-platforms --source-dir ".\obj\RMFImporter\Reclaimer" --output-dir ".\bin\"
}

function Copy-Artifacts {
    Write-Host 'Collecting Artifacts...' -ForegroundColor Yellow

    # copy installer MSI
    Get-ChildItem "$buildRoot\Reclaimer\Reclaimer.Setup\bin\Release" -Filter 'Reclaimer.Setup.msi' -Recurse `
        | Copy-Item -Destination ".\bin\Reclaimer.Setup-v$assemblyVersion.msi"
}


$ErrorActionPreference = 'Stop'

Clear-Host
Push-Location $PSScriptRoot


# this requires inkscape to be installed and requires icomake.exe to be in the resources folder
# icomake can be found at https://github.com/tringi/icomake

Set-Alias inkscape 'C:\Program Files\Inkscape\bin\inkscape.exe'
Set-Alias icomake '.\Resources\icomake.exe'


Write-Host 'Preparing directory...' -ForegroundColor Yellow
Remove-Item '.\obj', '.\bin' -Recurse -Force -ErrorAction Ignore
New-Item bin -ItemType Directory | Out-Null
New-Item obj -ItemType Directory | Out-Null
Copy-Item .\Resources .\obj\res -Recurse | Out-Null


$buildRoot = '..\..'

if ($TargetBranch) {
    $buildRoot = '.\obj'

    Write-Host 'Preparing source files...' -ForegroundColor Yellow
    git clone --quiet --branch $TargetBranch -- https://github.com/Gravemind2401/Reclaimer.git ".\obj\Reclaimer"
    git clone --quiet -- https://github.com/Gravemind2401/Studio.git ".\obj\Studio"
}

# generate version number
Push-Location "$buildRoot\Reclaimer"
$buildNumber = [int](git rev-list --count `@)
$assemblyVersion = "2.0.$buildNumber"
Pop-Location

$scriptVersion = (Select-String -Path $buildRoot\Reclaimer\Reclaimer.RMFImporter\Reclaimer\__init__.py -Pattern "'version': \((.+)\)").Matches[0].Groups[1].Value.Replace(' ', '').Replace(',', '.')

Write-Host "Reclaimer $assemblyVersion" -ForegroundColor Yellow -BackgroundColor Black
Write-Host "RMF Importer $scriptVersion" -ForegroundColor Yellow -BackgroundColor Black

Rasterize-ApplicationIcon
Rasterize-InstallerResources
Build-Installer
Build-ImportScript
Copy-Artifacts


Start-Process ".\bin\"


Pop-Location
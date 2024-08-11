param (
    [string] $TargetBranch = $null
)

function Rasterize-ApplicationIcon {
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

function Copy-Artifacts {
    Write-Host 'Collecting Artifacts...' -ForegroundColor Yellow

    # copy installer MSI
    Get-ChildItem "$buildRoot\Reclaimer\Reclaimer.Setup\bin\Release" -Filter 'Reclaimer.Setup.msi' -Recurse `
        | Copy-Item -Destination ".\bin\Reclaimer.Setup-v$assemblyVersion.msi"

    # copy RMF importer files
    Copy-Item "$buildRoot\Reclaimer\Reclaimer.RMFImporter\Reclaimer" ".\obj\RMFImporter\Reclaimer" -Recurse
    Copy-Item "$buildRoot\Reclaimer\Reclaimer.RMFImporter\README.md" ".\obj\RMFImporter\Reclaimer\README.txt"

    # cleanup python cache files
    Get-ChildItem ".\obj\RMFImporter\Reclaimer" -Directory -Recurse `
        | Where-Object { $_.Name -eq '__pycache__' -or $_.Name -eq 'tests' } `
        | Remove-Item -Recurse -Force

    # create RMF Importer plugin zip
    Compress-Archive ".\obj\RMFImporter\Reclaimer" '.\bin\RMFImporter.zip' -CompressionLevel Optimal
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

Write-Host "Reclaimer $assemblyVersion" -ForegroundColor Yellow

Rasterize-ApplicationIcon
Rasterize-InstallerResources
Build-Installer
Copy-Artifacts


Pop-Location
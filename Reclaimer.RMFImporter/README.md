# Blender Setup

### Version Compatibility
- Minimum supported version: 2.91
- Highest tested version: 4.1

### Installation Instructions
1. In Blender go to `Edit > Preferences > Add-ons` and click `Install`
2. Browse to and select the `RMFImporter.zip` file then click `Install Add-on`
3. In the Add-ons preferences, select the `Community` tab and scroll down to the add-on named `Import-Export: RMF format`
4. Enable the add-on
5. In the Preferences section below the add-on details, click `Install Dependencies`
    - This step requires administrator permissions. If an error occurs, you may need to close Blender then open it again using the `Run as adminstrator` option in the right-click menu.
6. Restart Blender after the dependencies have been installed
7. After restarting Blender you should now have a new menu option under `File > Import > RMF (.rmf)`

### Configuration
The RMF Importer has add-on preferences that can be managed by going to 'Edit > Preferences > Add-ons' then scrolling down to the the add-on named `Import-Export: RMF format` in the `Community` tab. If you expand the add-on details you can scroll down to view and edit the preferences for it.

These preferences determine the starting settings that the import window will use each time you import a file. Changing the settings in the import window is only effective for the duration of that import. Changes made in the import window will not save back to the preferences for the next import.


# 3ds Max Setup

### Version Compatibility
- Minimum supported version: 2021
- Highest tested version: 2021

### Installation Instructions
In 3ds Max the RMF Importer can either be run via the `Scripting > Run Script...` menu by browsing for the script each time, or you can set it up as an extra menu option under the `Import` menu.

#### Simple setup:
1. Extract the `Reclaimer` folder from `RMFImporter.zip` file
2. In 3ds Max go to `Scripting > Run Script...`
3. Browse to the extracted `Reclaimer` folder and choose the file named `import_rmf.py`
     - Note that this script is **not** standalone - the `Reclaimer` folder must **not** be renamed and all files under that folder are **required**

#### Setup as a menu option:
1. Extract the `Reclaimer` folder from `RMFImporter.zip` file
2. Place the `Reclaimer` folder in the `scripts` folder of your 3ds Max install directory
    - For example, the path to the scripts folder may be `C:\Program Files\Autodesk\3ds Max 2021\scripts`
3. In the `Reclaimer` folder, find the file under `\autodesk\resources\Macro_ImportRMF.mcr` and copy it into the `MacroScripts` folder of your 3ds Max install directory
    - For example, the path to the MacroScripts folder may be `C:\Program Files\Autodesk\3ds Max 2021\MacroScripts`
    - If 3ds Max was already open, you may need to restart it after this step
4. In 3ds Max, go to `Customize > Customize User Interface...` and select the `Menus` tab
5. Set the Group filter to `Main UI` and the Category filter to `File`
6. In the `Action` list, scroll down to `Import RMF`. Drag and drop the `Import RMF` action into the menu tree on the right to add it anywhere in the menu layout.
    - The recommended location is to drop it under `File > File-Import`
    - A new menu item would then appear under `File > Import > Import RMF`
7. Use the `Import RMF` menu option at any time to start the import process

### Configuration
Currently in 3ds Max the RMF Importer can only be configured via the import window. Changing the settings in the import window is only effective for the duration of that import. The import window will revert to the default settings on each subsequent import.

### Important Notes
In 3ds Max the RMF Importer uses OSL shaders for terrain blending and color change maps. These shaders will not preview correctly in the viewport unless you set it to `High Quality` mode.


# Bitmap Path Settings

### Bitmaps Folder
The `Bitmaps Folder` setting can be left blank if the rmf file and all of its bitmaps are in certain directory layout configurations.

For example, if you have a file called `masterchief.rmf` and it uses a bitmap called `objects\characters\masterchief\bitmaps\masterchief.tif` then the bitmap folder can be detected automatically in the following scenarios:
1.  The files have the same layout as a batch extract:
    - `.\folder\objects\characters\masterchief\masterchief.rmf`
    - `.\folder\objects\characters\masterchief\bitmaps\masterchief.tif`
2.  The bitmaps were batch extracted to the same folder as the rmf file:
    - `.\folder\masterchief.rmf`
    - `.\folder\objects\characters\masterchief\bitmaps\masterchief.tif`
3.  The bitmaps were batch extracted to the same folder as the rmf file (but in a subfolder by the same name as the rmf file):
    - `.\folder\masterchief.rmf`
    - `.\folder\masterchief\objects\characters\masterchief\bitmaps\masterchief.tif`

If you specify a value for the `Bitmaps Folder` then that path will always be checked first, but it will still look in the locations listed above as a fallback when the bitmaps are not found.

### File Extension
The `File Extension` setting can be specified with or without a leading `.` character. This setting can be left blank if the extension is `.png` or `.tif`.

If you specify a value for the `File Extension` then that extension will always be checked first, but it will still try the extensions listed above as a fallback when the bitmaps are not found.
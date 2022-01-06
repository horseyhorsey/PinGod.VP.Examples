# Export Game (Final steps)
---

This would be the final step but isn't necessary when developing / debugging games.

### Pre Export Checklist - Project
---

- Recordings / Playback / Overlays switched off?

### Pre Export Checklist - Visual Pinball
---

- VP Script change to `Debug=False`
- VP Script change path to `./GameExecutable` - If game is packaged with table in the same directory the player only has to run the game in Visual Pinball

*Helps to have a copy of the release table in the export path to test exports and keeping your debug table*

## Export on Command line
---

Use the bat files export for steps. You only need to build the full export once. The executable will always be the same size, but the pack will be different.

Executable can be packed using UPX - Ultimate Packer for eXecutables. 40mb > 10mb. Download and add UPX to your environment, the bat file will pack executables in the Exported directory.

### _build_export_full.bat
---

Will build the exe and pack for the game. 

### _build_export_pck.bat
---

Will build the pack part of your game. If you already have an exported executable you can just run this for updated version being faster.

### _build_upx_shrink_executable.bat
---

Shrinks Godots exported executable. Will only need to be done after the first full export, see above

## Changing Windows Icons

- Make icon with all size in one file
- Download RCEdit and add the path in `Editor Settings\RCedit`
- Change icon in the `Project Settings/Application/Config`

See https://docs.godotengine.org/en/3.2/getting_started/workflow/export/changing_application_icon_for_windows.html
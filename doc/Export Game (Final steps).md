# Export Game (Final steps)
---

This would be the final step but isn't necessary when developing / debugging games.

### Pre Export Checklist - Project
---

- Recordings / Playback switched off?
- Logging levels at minimum? 
- Write / Read states on for Visual Pinball

### Pre Export Checklist - Visual Pinball
---

- Script change to `Debug=False`
- Script change path to `./GameExecutable` - If game is packaged with table in the same directory the player only has to run the game in Visual Pinball

*Helps to have a copy of the release table in the export path to test exports and keeping your debug table*

## Export on Command line
---

- Running `godot --export "Windows Desktop"` will export the game to the `Exported` folder.
- There's also a `.bat` in the project runs the command above

## Changing Windows Icons

- Make icon with all size in one file
- Download RCEdit and add the path in `Editor Settings\RCedit`
- Change icon in the `Project Settings/Application/Config`

See https://docs.godotengine.org/en/3.2/getting_started/workflow/export/changing_application_icon_for_windows.html
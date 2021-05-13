# PinGod - BasicGameGodot
---

Table authors use this as a base or example for building games.

# Setup / Running game
---

- Download Godot, doesn't require installation. https://godotengine.org/. ( Version 3.3 as of this document )
- Rename the executable to `Godot` and add to Environment paths on your system
- Now on any command line you can use `Godot`. Run `godot` in this project folder to load the game window
- Running `godot -e` will open the project in the editor. Here you can run the game or play individual scenes

# Duplicating this project to new
---

- Copy this project
- Rename the `.csproj` and `.sln` to your new name.
- Rename references in files with the new name. (VSCode replace in files)

# User Data
---

Logs are small and will be here, along with any saves made in the game, files, etc.

`C:\Users\YOURNAME\AppData\Roaming\Godot\app_userdata\BasicGameGodot`

# Exporting Game when ready to ship
---

## Command line
---

Run the export preset already setup with this project. Game is export to `../Export/PinGod.BasicGame.exe`

`godot --export "Windows Desktop"`

This game exe can be run from VP script Controller with the `.Run` method instead of `.RunDebug`

## Editor
---

Run the project in Godot editor and export without debug options.

# Game Updates / Patches / DLC / Mods
---

Deliver the project to players then when one wants to add functionality or content later on, just deliver the updates via PCK files to the players.

Instead of shipping whole game over again. .Pcks can be made and imported by the game later on.

Resources like scenes, scripts etc can be exported into pack from the export menu and on the resources tab.

https://docs.godotengine.org/en/stable/getting_started/workflow/export/exporting_pcks.html#use-cases
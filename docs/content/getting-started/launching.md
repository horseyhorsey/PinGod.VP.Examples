---
title: "Launching"
date: 2017-10-17T15:26:15Z
lastmod: 2019-10-26T15:26:15Z
draft: false
weight: 20
---

# Launching BasicGameGodot

### `_create_addons_link.bat`

Run to create a symbolic link to the addons directory in the `BasicGameGodot` project or in any game. 

You can copy the addons directory to your project if you needed to though. To run the BasicGame you will need the addOns directory linked or copied.

### BasicGame

The `BasicGameGodot` project can be loaded in the Godot editor by browsing in Godot but it is quicker to load into the editor via the file explorer or command line.

- Launch Game   =  `godot` in the project folder on command line to play the game in window.

- Launch Editor = `godot -e` in the project folder to launch editor.

## Project Bat files

You can use the `.bat` files to launch which use the following commands:

### `_run_godot_editor.bat` 

Launch the project in Godot Editor mode

### `_open_appdata_dir.bat`

Opens the `app_userdata` for the project. Logs go here, `gamedata.save`, `settings.save` and `recordings`
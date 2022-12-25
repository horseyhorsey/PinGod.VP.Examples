---
title: "Main Scene"
date: 2017-10-17T15:26:15Z
draft: false
weight: 10
---

This scene is the first to be loaded as a base display set in the projects settings and the scene is never removed from the game, but can add / remove modes like removing the Attract then adding a Game scene.

The first scene can also be found in the `project.godot` file. `run/main_scene="res://game/MainScene.tscn"`

The PinGodAddOns test project loads a Node2D scene instead `run/main_scene="res://Node2D.tscn` and doesn't use a base script.

## BasicGame - MainScene.tscn

#### Used By

- PinGodGame = To control window settings from game


### Default Modes in tree

You can create your scenes using these script class as a base and copying the scene from the addons and adjusting and making your own.

### Attract (Mode)
---

Standard to show high scores, push start. When pushing start in this mode when the `Trough` is ready that will remove the attract mode and add the `Game.tscn`.

Default scene `addons/PinGodGame/Scenes/Attract.tscn`

### PauseControl
---

A layer to show when the game is paused. 

Default scene `addons/PinGodGame/Scenes/PauseLayer.tscn`

### Settings Display
---

In game menu to adjust audio, display, game and pingod settings.

Default scene `addons/PinGodGame/Scenes/Menus/SettingsDisplay.tscn`


![image](../../images/mainscene.jpg)


Base scene script for a MainScene or first scene in project. Overrides the Node2Ds `_EnterTree` to setup the scene, get references and connect signals.

Node2D `addons/PinGodGame/Game/MainScene.cs`

### Referenced By

- PinGodGame = To control window settings from game

### Exports

***`[Export]` attributes on a property can be used inside Godot editor UI.***

---

`_game_scene_path` = The Game Scene to load. Default: res://game/Game.tscn

`_service_menu_scene_path` = The Service Menu Scene to load. Default: `res://addons/PinGodGame/Scenes/ServiceMenu.tscn`

### _EnterTree (Override)
---

#### Signals connected

- PinGodGame.GameStarted		= Calls StartGame which loads the Game scene
- PinGodGame.GameEnded			= Removes the `Game` from Modes tree.
- PinGodGame.ServiceMenuExit	= Reloads the scene. `ReloadCurrentScene`

#### Nodes referenced

- Modes/Attract
- CanvasLayer/PauseControl
- SettingsDisplay

#### PauseMode = Process

### _Ready (Override)
---

Hides pause menu and sets the `died` solenoid to 1 so other integrations know it's alive.

### _Input (Override) - Actions
---

- pause 	= Toggle Pause Menu
- quit		= Invoke PinGodGame.Quit 
- settings	= Display settings, will pause game if not paused
- enter		= Enter will load service menu and remove Game or Attract if in Modes tree



![image](../../images/mainscene.jpg)
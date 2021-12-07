# PinGod - RolloverLanesGodot
---

#### ⚙️ Setup
---

##### PinGod/AddOns path
---

In this game directory add a symbolic link to where the `addons` directory is. 

*Full path is necessary*. *You can also just copy the addons folder.*

```
mklink /D addons "C:\Users\funky\source\repos\PinGod\PinGod.VP.Examples\addons\addons"
```

#### ❓ About
---

##### Common and Autoloaded Scenes
---

###### res://game/PinGodGame.tscn
---

Use this scenes inspector to view / change settings, machine configurations. This scene should be autoloaded (set to by default)

*Visual Pinball needs read/write states enabled*

###### res://game/MainScene.tscn
---

This scene is the first to start when the game is loaded. This houses the `AttractMode`.

###### res://modes/Game.tscn

#### Godot Scene / UI / Docs
---

See Godot documentation for general scene creating, project settings and everything else. https://docs.godotengine.org/en/stable/getting_started/step_by_step/scripting.html#introduction

#### RolloverLanesGodot Step by Step log
---

##### Add node in Godot
---

1. Added `switches` and `leds` to the machine under `"lane_num"` in the `res://game/PinGodGame.tscn` scene
2. Created new `User interface` scene named `res://modes/custom/RolloverTopLanes.tscn`
3. Created `C#` script in the same directory. `res://modes/custom/RolloverTopLanes.cs`
4. Changed the scripts base class from `Control` to `PinballSwitchLanesNode`
5. Built Godot project to show inherited options in the `RolloverTopLanes` inspector
6. Instanced as child scene (`RolloverTopLanes`) in the `Game.tscn` scene above the `BaseMode` scene
7. Added the `Led Lamps` and `Lane Switches` names into `RolloverTopLanes` inspector
8. Added the scene into the `Mode` GROUP (from insector). The game will call UpdateLamps then when needed to this group.

This is enough to run the mode in Godot for testing with overlays or keys, but we should act on what to do after the lanes are completed.

##### Scripting - RolloverLanes.cs
---

1. Overridden the `CheckLanes` method.
2. Check if completed here and if so we invoke `ResetLanesCompleted` and `Updatelamps`

##### Visual Pinball Table
---

See Visual Pinball directory

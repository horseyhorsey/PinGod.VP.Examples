# PinGod - MotuGodot
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
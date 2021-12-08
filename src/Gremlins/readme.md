# PinGod - BasicGameGodot
---

A basic example for building games in Godot with PinGod for Visual Pinball.

## Setup path before running in Godot
---

In this directory add a symbolic link to where the `addons` directory is. *Full path is necessary*. *You can also just copy the addons folder.*

```
mklink /D addons "C:\Users\funky\source\repos\PinGod\PinGod.VP.Examples\addons\addons"
```

## Godot Scene / UI / Docs
---

See Godot documentation for general scene creating, project settings and everything else. https://docs.godotengine.org/en/stable/getting_started/step_by_step/scripting.html#introduction

### MainScenes / Nodes
---

- PinGodGameBase
- PinGodGame.tscn
- MainScene
- Game

See documentation directory for better explanation of these scenes

## Default window keys (Actions)
---

- Coin = 5
- Start = 1
- Trough Switches = Q, W, E, R
- Flippers = \, / - Can't use L-Shift and RShift
- Tilt = X
- Slam Tilt = END

See the `InputMap` tab in project settings

## Common Pinball - Basic Modes / Placeholders
---

- Attract
- Ball Saves
- Ball Search
- Bonus (End of ball display)
- Multiball
- Pause
- Score Entry
- Score Mode
- Service Menu (TODO)
- Tilt
- Trough

# Logging / GameData / Settings
---

Can view most of it from the debug console window but if you miss something they can be found at `C:\Users\YOURNAME\AppData\Roaming\Godot\app_userdata\BasicGameGodot`.

Opening the folder in editors like `Sublime, VsCode` is a quick way to view your `SaveData`, `Settings` and `Logs`

![image](../../../doc/images/sublime-userdata-folder.jpg)
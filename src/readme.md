# Examples

Examples for building games in PinGod and Visual Pinball simulator. Below are some steps needed per game and defaults.


## Building / Running

In each project directory there is a `Godot` project and a `Visual Pinball` Table / Script project.

### Godot
---

A setup of `Godot` portable installed on your system which can be found here [Godot Download](https://godotengine.org/download)

Before building a godot project you can either copy or link the `addons` shipped with this repository.

#### PinGod Addons
---

Run the `_create_addons_link.bat` to create a symbolic link to PinGodAddons or in a game project directory run the following command to link the addons

`mklink /D addons "C:\YourFullPathToTheAddons\PinGod.VP.Examples\addons\addons"`

This directory could also be copied instead of linking.

### Visual Pinball
---

- Table examples are targetting `VPX 10.7`

[PinGod.VP.Controller](https://github.com/horseyhorsey/PinGod.VP/releases) setup (used for communicating with PinGod) 


## Logging, GameData, Settings
---

All projects store small log, settings and gamedata, you can find at the following path on your machine

`C:\Users\YOURNAME\AppData\Roaming\Godot\app_userdata\{GameProjectName}`

### Defaults
---

#### Keys / Switches (Godot Window)
---

| Switch       | Keys  | Note |
|--------------|:-----:|
| Coin    |  2 |
| Start    |  1 |
| Flippers         |  \\ , /  | Can't use L-Shift and RShift |
| Slam Tilt    |  END |
| Tilt         |  X |
| Trough         |  Q, W, E, R |

Keys can be assigned to switch actions in godots `Project Settings/InputMap`

# PinGodVp-BasicGame-VPX10-6
---

Visual pinball table for game. Requires registered controller.

https://github.com/horseyhorsey/PinGod.VP

## About table

Stripped all sounds and textures to try and make table small as possible.

The table script will run with the `Example` default table that you can create inside Visual Pinball. 

## Running
---

Table can be run from any location in VP.

- Register the `PinGod.VP controller`. https://github.com/horseyhorsey/PinGod.VP .
- Add the extra scripts needed from the controller install to `VisualPinball/Scripts`
- Update the tables script to include the path to the Godot project. `Const GameDirectory = "C:\Yourfilepath\BasicGameGodot"`

The controller will load the game when launched. When the game is ready a signal is sent back to visual pinball which will activate the game.

## Controls
---

Uses standard VP player inputs.

This table uses a version of the `core.vbs` to do general machine stuff. Default switches here are setup in the Godot projects `InputMap`.

The default attract mode uses `2` for credits and start as `19`, flippers too, see below.

```
Cabinet switches
Const swCoin1  = 0
Const swCoin2  = 1
Const swCoin3  = 2
Const swCoin4  = 3
Const swCancel = 4
Const swDown   = 5
Const swUp     = 6
Const swEnter  = 7
Const swCoinDoor  = 8
Const swLLFlip = 9
Const swLRFlip = 11
Const swULFlip = 13
Const swURFlip = 15
Const swSlamTilt = 16
Const swTilt = 17
Const swStartButton = 19
```

## Controller (Script access)
---


### Switch - (Manual)
---

Switch events can be sent with `Controller.Switch 20, 1`. Usually this is `Controller.Switch(20) = 1`.

### Switch - (Auto)
---

`vpmCreateEvents AllSwitches` is called when table loads. This runs updated switches from this collection.

The switch number is set inside the VP Triggers `TimerInterval` variable

### Lamps
---

`vpmMapLights AllLamps` is called when table loads. This runs updated lamps from this collection.

The lamp number is in the VP Light objects `TimerInterval`

### Action
---

You can send any action to Godot here from `Controller SetAction "my_action", 1`.

The controller sends `pause` and `quit` via this actions. See the `InputMap` in the godot project to create your own.






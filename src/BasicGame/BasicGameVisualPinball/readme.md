# PinGodVp-BasicGame-VPX-10.7
---

Visual pinball table for BasicGameGodot. Requires controller setup [PinGod.VP](https://github.com/horseyhorsey/PinGod.VP/releases)

## Running
---

Table can be run from any location in Visual Pinball. The controller will load the game when the vp player is launched. When the game display is ready, a signal is sent back to Visual Pinball, which will activate the Visual Pinball Player.

### Game Project Setup
---

When running the table with the games Godot project relative to the table (the same as this repo), then you won't have to adjust the script for the `GameDirectory`.

Change the default to `Const GameDirectory = "..\BasicGameGodot"` to `Const GameDirectory = "C:\Yourfilepath\BasicGameGodot"` if needed.

## Display settings
---

Display can be adjusted by using `F2` on the window, move and resize and save the position.

```
.DisplayX = 1920 - 512
.DisplayY = 10
.DisplayWidth = 512 ' 1024 FS
.DisplayHeight = 300 ' 600  FS
.DisplayAlwaysOnTop = True
.DisplayFullScreen = False 'Providing the position is on another display it should fullscreen to window
.DisplayLowDpi 	= False
.DisplayNoWindow = False
```	

See table script PinGodVp-BasicGame-VPX10-7.vbs for more.

## Controls / Keyboard
---

Controls in Visual Pinball are standard inputs for flippers and other machine switches, etc. 

## Default Machine Switches

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

## Script Notes / Guide
---

- Similar to PinMame machine scripts
- Switch events can be sent with `Controller.Switch 20, 1`. Usually this is `Controller.Switch(20) = 1`.
- Checks for led updates with the `PinMameTimer`. `Const UsePdbLeds = True`
- Check for coil updates with the `PinMameTimer`. `Const UseSolenoids = 1`
- `vpmMapLights AllLamps` is called when table loads. This runs updated lamps from this collection. The lamp number is in the VP Light objects `TimerInterval`
- `vpmCreateEvents AllSwitches` is called when table loads. This runs updated switches from this collection. The switch number is set inside the VP Triggers `TimerInterval` variable

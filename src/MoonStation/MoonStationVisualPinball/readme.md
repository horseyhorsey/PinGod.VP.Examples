# PinGodVp-BasicGame-VPX-10.7
---

Visual pinball table for BasicGameGodot. Requires controller setup [PinGod.VP](https://github.com/horseyhorsey/PinGod.VP/releases)

## Running

Table can be run from any location in Visual Pinball. The controller will load the game when the vp player is launched. When the game display is ready, a signal is sent back to Visual Pinball, which will activate the Visual Pinball Player.

## Display settings

Display can be adjusted by using `F2` on the game display window, move and resize and save the position.

```
.DisplayX = 1920 - 512
.DisplayY = 10
.DisplayWidth = 512 ' 512 - original width
.DisplayHeight = 384 ' 384 - original height
.DisplayAlwaysOnTop = True
.DisplayFullScreen = False 'Providing the position is on another display it should fullscreen to window
.DisplayLowDpi 	= False
.DisplayNoWindow = False
```	

See table script PinGodVp-BasicGame-VPX10-7.vbs for more.

## Controls / Keyboard

Controls in Visual Pinball are standard inputs for flippers and other machine switches, etc. 


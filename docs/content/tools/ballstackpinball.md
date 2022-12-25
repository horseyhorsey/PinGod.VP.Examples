---
title: "BallStackPinball (Timer)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

Basic way to use a Timer as a Saucer or Kicker, the scene using this control should connect to the switch signals sent from them. See BasicGame

Tool: BallStackPinball derives from a Timer and can be used in scenes and adding them from filtering in the Godot editor.

## Exports

Coil		= coil sname to pulse
Switch		= switch name that activates
One Shot	= true
Autostart	= false

## Godot - Overrides

`BALL_SAVE_SCENE` = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

### _EnterTree

- Gets reference to `PinGodGame` to be able to check input actions, ie: switches

### _Ready

Disables input processing if the _switch wasn't given

### _Input

Processes the switch name given to this Timer and emits signals `SwitchActive` or `SwitchInActive`

## Methods

SolenoidPulse = 	Pulse the coil for this Timer

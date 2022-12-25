---
title: "BaseMode (BasicGame)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

This BaseMode scene is loaded in the `Game.tscn` Modes scene used in the `BasicGame` project. A basic mode which has a saucer (kicker) and starts multiball when active.

## BaseMode.tscn

### Exports

`BALL_SAVE_SCENE` = "res://addons/PinGodGame/Modes/ballsave/BallSave.tscn";

### BallStackPinball

In Node / Signals pane we have connected signals to the `BallStackPinball` scene

![image](../../images/basemode_signals.jpg)

Here's what it looks like connected inside a `.tscn`. The methods are inside the `BaseMode.cs` script.

```
[connection signal="SwitchActive" from="BallStackPinball" to="." method="OnBallStackPinball_SwitchActive"]
[connection signal="timeout" from="BallStackPinball" to="." method="OnBallStackPinball_timeout"]
```

#### Signals

- OnBallStackPinball_SwitchActive
- OnBallStackPinball_timeout

## BaseMode.cs - Overrides

### _EnterTree

- Gets reference to `BasicGame` scene which is the parent of the Modes layer (this scenes parent). `game = GetParent().GetParent() as BasicGame;`
- Loads a packed scene from `BALL_SAVE_SCENE`. This scene is added when called from `DisplayBallSaveScene`
- Gets reference to for the `_ballStackSaucer`. A [[BallStackPinball|06b.BallStackPinball]] Timer. 
- The `_ballStackSaucer` is used to start multiball.

### _Input

Processes the following actions if `PinGodGame.GameInPlay` and not `PinGodGame.IsTitled`:

- start button	= calls `pingod.StartGame`
- flippers		= nothing
- outlanes		= add 100 points
- inlanes		= add 100 points
- slingshots	= add 50 points

### Methods

- OnBallSaved = Displays a ball save scene for 2 seconds if not in multi-ball. `DisplayBallSaveScene`
- OnBallStackPinball_SwitchActive = Sets `PinGodGame.IsMultiballRunning` and  `BasicGame.AddMultiballSceneToTree` to add multiball to the tree

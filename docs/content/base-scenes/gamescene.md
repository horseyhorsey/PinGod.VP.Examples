---
title: "Game Scene"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

This scene is loaded from the attract mode. (Add to favorites for quick access)

Add scenes to the modes CanvasLayer to use in game or add to from script

![image](../../images/game_scene.jpg)

## Default Modes (CanvasLayer)

### ScoreMode

[[ScoreMode.tscn ScoreMode.cs|06a.-ScoreMode]]

### BaseMode

- Set the `Ball Save Scene`

[[BaseMode.tscn BaseMode.cs|06b.-BaseMode]]

### Bonus

- Set the default text and display time

[[Bonus.tscn Bonus.cs|06c.-Bonus]]

### Tilt

- Set number of tilt warnings

[[Tilt.tscn Tilt.cs|06d.-Tilt]]


### ScoreEntry

- Scene to show when player is entering a score

[[ScoreEntry.tscn ScoreEntry.cs|06e.-Tilt]]


Base Game scene for `Game.tscn`. `BasicGame.cs` inherits this class and is used in the `game/Game.tscn` scene.

You can add some of your own overrides here to existing methods at common stages of a pinball game. See Methods to override below.

This is a good scene to invoke methods on groups. Used in `res://addons/PinGodGame/Game/Game.cs`

## Exports

`MULTIBALL_SCENE` 			= Default scene file to use for Multi-Ball: `res://addons/PinGodGame/Scenes/Multiball.tscn`

`_service_menu_scene_path` 	= The Service Menu Scene to load. Default: `res://addons/PinGodGame/Scenes/ServiceMenu.tscn`

## Godot - Overrides

### _EnterTree
---

Creates a tilted timer which starts a new ball on timedout

#### Nodes (Scenes) referenced

- Modes/ScoreEntry
- Modes/Bonus

#### Signals connected

- `PinGodGame.BallDrained`		= `PinGodGame.OnBallDrained(SceneTree, string, string)` on the whole tree so any modes in group named `Mode` with `OnBallDrained` will be called.
- `PinGodGame.BallEnded	`		= Ends multiball. Starts Bonus if not tilted. Displays high score if last ball
- `PinGodGame.BallSaved	`		= PinGodGame.OnBallSaved on modes group as node in tree. `Mode` group.
- `PinGodGame.BonusEnded`		= Displays high score entry `scoreEntry.DisplayHighScore` if last ball or `StartNewBall`
- `PinGodGame.MultiBallEnded`	= `EndMultiball` on all scenes in the `multiball` group
- `PinGodGame.ScoreEntryEnded`	= `PinGodGame.EndOfGame`, which will send a game ended

### _Ready

Sets the games `BallInPlay = 1` and starts a new ball

## Methods to Override

- AddMultiballSceneToTree	= Gets an instance of the multi-ball scene and add it to the Modes tree in group `multiball`
- AddPoints					= add points (pinGod.AddPoints) with 25 bonus
- EndMultiball				= Any node that is in the multiball group is removed from tree
- OnBallEnded				= Add a display at end of ball
- OnBonusEnded				= Displays high score if this is the last ball.
- OnScoreEntryEnded			= pingod.EndOfGame
- StartNewBall				= Starts new ball with PinGodGame and invokes the `OnBallStarted` method on all in `Mode` group
- OnBallDrained				= PinGod.OnBallDrained invokes the `OnBallDrained` method on all in `Mode` group
- OnBallSaved				= Calls `OnBallSaved` for the whole tree in `Mode` group

---
title: "PinGodGame.cs - Script"
date: 2017-10-17T15:26:15Z
draft: false
weight: 41
---

This script is autoloaded from the scene when the display window is launched and has many methods.

Modes and other scenes can get access to this from the `root` tree.  `GetNode<PinGodGame>("/root/PinGodGame")`.

## Godot - Overrides

`_EnterTree` = Gets user cmd line args, loads data and settings, creates trough, sets up ball search and audio manager

`_Ready` = Game initialized. `Memory map` is created here if read and write is enabled. Gets `BallSearchOptions`, sets up a `_lampMatrixOverlay` Gets hold of the MainScene to control window size, stretch

`_ExitTree` = Saves recordings if enabled and runs Quit(bool)

`_Input` = Listens for any escape action preses. Handles coin switches, adds credits. Toggle border F2 default

`_Process` = Processes playback events...Processing is disabled if it isn't enabled or if playback is finished

## Signals (Game Events)

#### Signal list
- BallDrained
- BallEnded(bool lastBall)
- BallSaved
- BallSaveEnded
- BallSaveStarted
- BonusEnded
- CreditAdded
- GameEnded
- GamePaused
- GameResumed
- GameStarted
- GameTilted
- ModeTimedOut(string title)
- MultiBallEnded
- MultiballStarted
- PlayerAdded
- ScoreEntryEnded
- ScoresUpdated
- ServiceMenuEnter
- ServiceMenuExit
- VpCommand(byte index)

Signals can be found in `PinGodGameBase.cs`. The game has to have these or at least inherit them to be able to be emitted.

You can connect to signals from a `PinGodGame` scene reference, See `res://addons/PinGodGame/Game/Game.cs` in `_EnterTree` for example.

`pinGod.Connect(nameof(PinGodGame.BallDrained), this, nameof(OnBallDrained));`

## Some example methods

Pretty much all can be overriden in this class. 

Generate documentation from source see [Setup-Generate Docs](../wiki/01.-Setup) to view all methods in `PinGodGame`

`AddBonus(long points)` = Add bonus to current player

`AddCredits(byte amt)` = Add game credits

`AddPoints(long points)` = Add points to current player

`BallsInPlay()` = Gets balls in play from the _trough

`DisableAllLeds`, `DisableAllLamps` = Disable all lights

`EnableFlippers	(byte enabled)` = enable flippers?

`IsSwitch (string[] switchNames,InputEvent input)` = Detect if the input isAction found in the given switchNames. Uses SwitchOn 

`LogInfo,LogWarning,LogError,LogDebug` = write to logs

`StartMultiBall	(byte numOfBalls, byte ballSaveTime = 20,float 	pulseTime = 0)` = Sets MultiBall running in the trough and Emits MultiballStarted

`StopMusic()` = Stops the music in audiomanager

`SwitchOn(string swName)` = Checks a switches input event by friendly name. If the "coin" switch is still held down then will return true

`UpdateLamps` = Invokes UpdateLamps on all groups marked as Mode within the scene tree. scene tree CallGroup

## BasicGame usage

### CustomPinGodGame

The `BasicGameGodot`s `CustomPinGodGame` class inherits `PinGodGame` and is set as a script in the `PinGodGame.tscn`. So while the `PinGodGame.tscn` scene is set to autoload with a project, then any custom game classes can be by selecting it in the godot editor( safer) or by editing the `PinGodGame.tscn` file and changing script there.

- Overrides the `CreatePlayer` to create a `BasicGamePlayer` and doesn't call the base method.

- Overrides the Setup but calls the base method.

### Game.cs

In this scene overriding the `_EnterTree` uses `pinGod` to log messages and connect to some signals. 
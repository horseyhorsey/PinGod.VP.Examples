---
title: "Bonus"
date: 2017-10-17T15:26:15Z
draft: false
weight: 10
---

Bonus scene used in BasicGame `Game.tscn` Modes.

StartBonusDisplay is used from the `Game.OnBallEnded` if the game isn't tilted. So you can override this with your own.

![image](../../images/bonus.jpg)

## Scene Tree breadown

backgroundColor
pingod-logo = sprite
Label
Timer = 1 second

## Exports

- Default Text
- Display For Seconds

## Overrides

### _EnterTree

- sets Timer and Label
- If no _defaultText is available it uses translated `BONUS_EOB`

### _Ready

- Finds Trough scene
- Gets Timer and BlinkingLabel from scene tree

### _Input

- Watches actions for `tilt`, `slam_tilt`
- Both tilting actions disable flippers and ball saves.
- Show a message how many warnings or if tilted on top of everything else

## Methods

- OnTimedOut = Stops the timer, hides the scene and emits the BonusEnded signal
- SetBonusText(string)
- StartBonusDisplay(bool visible = true) = Starts display for the amount of seconds set

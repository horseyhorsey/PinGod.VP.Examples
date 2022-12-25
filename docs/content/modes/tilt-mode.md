---
title: "Tilt Mode Scene"
date: 2017-10-17T15:26:15Z
draft: false
weight: 10
---

Base Tilt scene to act on any machine tilting actions and displaying

## Scene Tree breadown

background
CenterContainer > `BlinkingLabel` = Shows the tilt messages
Timer = 2 second One Shot timer

## Exports

Num Tilt Warnings = Warning before titlt , default = 2

## Overrides

### _Ready

- Finds Trough scene
- Gets Timer and BlinkingLabel from scene tree

### _Input

- Watches actions for `tilt`, `slam_tilt`
- Both tilting actions disable flippers and ball saves.
- Show a message how many warnings or if tilted on top of everything else

## Methods

- SetText(string)
- ShowTilt()
- ShowWarning()
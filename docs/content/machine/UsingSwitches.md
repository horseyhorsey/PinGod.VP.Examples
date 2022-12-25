---
title: "Using Switches (Godot)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 30
---

### Using in game scene scripts

Override `_Input(InputEvent @event)`

Check if a switch has just been triggered on / off. This mode would use a pinGod game reference to do so.

```
//pinGod is a reference to the PinGodGame
if (pinGod.SwitchOn("inlane_l", @event))
{
    AddPoints(100);
}
```

#### Check a switch state anywhere

```
bool isInlaneOn = pinGod.SwitchOn("inlane_l");

```
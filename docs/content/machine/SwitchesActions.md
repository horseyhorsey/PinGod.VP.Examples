---
title: "Switches to Actions"
date: 2017-10-17T15:26:15Z
draft: false
weight: 10
---

### Switches

Godot uses Actions so you can assign input controls to them.

In the projects `Input Map` add switches like the example below.

![image](../../images/godot-input-actions.jpg)

Switch numbers convert to these actions `sw{number}` and you can assign keyboard to test game switches directly in the window.


### About

An action comes into Godot like this so we know it's a switch and act on the switch name

You can create any action here and trigger it from a simulator controller. See `pause` and `quit` which is assigned to ESC but also used externally.

The following are the default switches. These were copied by opening the `PinGodGame.tscn` in a text editor, you can add them here if you find it faster than godot inspector.

```
_switches = {
"coin": 2,
"down": 5,
"enter": 7,
"exit": 4,
"flipper_l": 11,
"flipper_r": 9,
"inlane_l": 22,
"inlane_r": 23,
"mball_saucer": 27,
"outlane_l": 21,
"outlane_r": 24,
"plunger_lane": 20,
"slam_tilt": 16,
"sling_l": 25,
"sling_r": 26,
"start": 19,
"tilt": 17,
"trough_1": 81,
"trough_2": 82,
"trough_3": 83,
"trough_4": 84,
"up": 6
}
```
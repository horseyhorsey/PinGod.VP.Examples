---
title: "Using Coils (Controller)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 51
---

## Visual Pinball SolCallbacks

When Visual Pinball detects changed coils the callbacks are invoked by coil number.

The script example to handle coil `35` would be `SolCallback(35) = "Lampshow2"`. This will invoke the `LampShow2` sub routine (if you have one)

Some standard callbacks for default game:

```
SolCallback(0) = "Died"  ' If at some point Godot closes then this lets VP know about it
SolCallback(1) = "bsTrough.solOut" ' The trough which makes the ball pop out....
SolCallback(2) = "FlippersEnabled" ' Enable and disable flippers, when ball drains, tilt etc
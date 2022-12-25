---
title: "Using Switches (Controller)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

### (Scripting) Visual Pinball - Switch On / Off

```
Sub sw_plunger_lane_hit() : Controller.Switch 20, 1 :  End Sub   
Sub sw_plunger_lane_unhit() : Controller.Switch 20, 0 :  End Sub
Sub sw_spinner_spin() : Controller.Switch 21, 0 :  End Sub
```

### Auto Switch handlers (No scripting)
---

In the BasicGame, Visual Pinball table switches are added to a simulator collection named `AllSwitches`.

Add the switch number to the `TimerInterval` box in the objects settings and add the switch to the `AllSwitches` collection.

* All objects should be covered, so you wouldn't have to remember to use `Spin` instead of `Hit` for a spinner in the collection.

![image](../../images/vp-auto-switches-1.jpg)

Doing this allows us to remove the need for typing in switch handlers in the script like this `sw_plunger_lane_hit` and automate, that's if we really don't need to do anything else from the script when the switch is triggered.


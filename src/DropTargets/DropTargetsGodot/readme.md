# PinGod - DropTargetsGodot
---

## Add To Machine config

### Switch and Leds
---

- Add in 5 new switches and lamps. This can be done from the `PinGodGame` inspector or editing the `res://game/PinGodGame.tscn` file and adding.
- Create `target1 = 30`, `target2 = 31` and so on for the switches, same for leds (can be lamps but BasicGame is for led).

```
"target1" : 30,
"target2" : 31,
"target3" : 32,
"target4" : 33,
"target5" : 34,
```

- Leds look the same, we just used the same number in this instance

```
_leds = {
"shoot_again": 1,
"target1" : 30,
"target2" : 31,
"target3" : 32,
"target4" : 33,
"target5" : 34,
}
```

### Add Coil / Solenoid to reset targets
---

Same as above but we add in `"dt_reset" : 5` into the `coils` collection

## Godot Targets Scene

### Godot Scene and Script Creation
---

- Create a new scene named `DropTargetsMode` from right clicking inside the `modes/custom` folder
- Choose `User Interface` for the node
- Rename the control in the `Scene` tree to `DropTargetsMode`
- Click the `Attach Node Script` button, select language C# and the path should be auto set to where your scene is at `modes/custom/DropTargetsMode.cs`
- This will create a new script file. Highly recommended to setup `VSCode` or `Visual Studio` for scripting or any other edito that can do C# rather using godot script editor.


### Adjusting Script
---

- By default the class parent is a `Control`, change this to a `PinballTargetsControl`

```
    public class DropTargetMode : PinballTargetsControl
    {

    }
```

### Adding target switches and lamps
---

- Build the project then select the `DropTargetModes` in the `Scene` tree
- In the `Inspector` you will see `Target Switches`, `Target Lamps` and `Inverse Lamps` which come from the `PinballTargetsControl`
- Add in the switch and lamp/led names from the machine config into these collections

## Add the Mode to Game Scene
---

- Open the `res://modes/Game.tscn` scene (should be in the favorites section for quick access)
- Right click the `Modes` layer and `Instance Child Scene` and select the `DropTargetMode.tscn`
- Move this layer above `BaseMode` and below the `Bonus`
- In the inspector for this scene add into a group named `Mode` 

### Overriding PinballTargetsControl methods
---

Some methods were overriden, including `UpdateLamps`, see the [DropTargetMode.cs](/src/DropTargets/DropTargetsGodot/modes/custom/DropTargetsMode.cs) in this repo.

#### SetTargetComplete(int index)

#### CheckTargetsCompleted(int index)

#### TargetsCompleted(bool reset = true)
---


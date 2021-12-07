# PinGodVp-DropTargets-VPX10-7
---

## Script edits before playing
---

- Change `GameDirectory` to match where the source code is for the game `DropTargets\DropTargetsGodot`

### Steps to create drop targets
---

- Add drop target objects to the table in the Visual Pinball editor (the names are below, Target1 etc)
- Add a variable at top of script named `Dim dtBank`
- Add a new `cvpmDropTarget` inside the `InitGame` method

```
	Set dtBank = New cvpmDropTarget
		With dtBank
		.InitDrop Array (Target1, Target2, Target3, Target4, Target5), array (30,31,32,33,34) ' map objects to switch numbers
		.InitSnd "DropTarget", "fx_resetdrop"
		.CreateEvents "dtBank"
		End With
```		

- Add a solenoid handler `SolCallback(5) = "dtBank.SolDropUp"`

### Create Target Lamps
---

- Create 5 lamps and assign each the lamp number in the Lights `Timer Interval`
- The interval should be disabled and is just used to store the number
- Add the lamps to the `AllLamps` collection

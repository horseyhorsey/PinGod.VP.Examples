# PinGod - SpinningDiscsGodot
---

#### ⚙️ Setup
---

##### PinGod/AddOns path
---

In this game directory add a symbolic link to where the `addons` directory is. 

*Full path is necessary*. *You can also just copy the addons folder.*

```
mklink /D addons "C:\Users\funky\source\repos\PinGod\PinGod.VP.Examples\addons\addons"
```

#### Changes from BasicGame

- Added coil for disc. `spinning_disc` to number 5 in the `res://game/PinGodGame.tscn` scene

#### BaseMode

- Updated the `StartMultiball` method to start spinning when that starts. `pinGod.SolenoidOn("spinning_disc", 1);`
- Updated the `OnBallDrained` method to stop the spinning when all balls have drained and multiball is over `pinGod.SolenoidOn("spinning_disc", 0);`
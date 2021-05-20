# PinGod - BasicGameGodot
---

Table authors use this as a base or example for building games in Godot for Visual Pinball.

See Godot documentation for general scene creating, project settings and everything else. https://docs.godotengine.org/en/stable/getting_started/step_by_step/scripting.html#introduction

## Setup before running

In this directory add a symbolic link to where the addons directory is. *Full path is necessary*

```
mklink /D addons "C:\Users\funky\source\repos\PinGod\PinGod.VP.Examples\addons\addons"
```

## Common Pinball - Basic Modes / Placeholders

- Attract
- Ball Saves
- Bonus (End of ball display)
- Pause
- Score Entry - TODO
- Score Mode
- Tilt
- Trough

## MainScene
---

This should be looked at as a master scene which controls the game and scene changes etc and never removed. Entirely up to you of course.

`StartGame` will remove the `attract mode scene` and load the Game scene into the Modes tree.

`EndGame` is invoked after trough checks in the actions where this will reload the entire `MainScene` scene removing the game and adding back in the attract.

Pause mode is inside this scene and listens for the `pause action`. `@event.IsActionReleased("pause")`

## Autoloaded Scenes / Scripts
---

You can find any of these in the project settings inside Godot. `Project Settings/Autoload`

### GameGlobals.cs
---

Holds game specific variables. `BallsPerGame` `Players` and the like can be found for pinball as a base.

### Trough.cs
---

Basic handling of a trough in a pinball machine with added ball save. Ball saving requires a `plungerlane` switch number.


### OscService.cs
---

The service that handles requests from the PinGod.VP.Controller on local machine loopback. Recording playback TODO.

#### Receiver (default port 9000)
---

This listens on the `/action` address. An action name and pressed number is sent to this.

The `quit` action is handled in this class to close the app on request.

#### Sender port (default port 9001)
---

This sends to multiple addresses. The helper methods like `SetCoilState`, `SetLampState` use these and can be used from the other modes.

- `/evt`    == Sends an event. `game_ready` is an example to the controller.
- `/coils`  == Sends a coil state change
- `/lamps`  == Sends a lamp state change

---


# Logging.
---

Can view most of it from the debug console window but if you miss something they can be found at `C:\Users\YOURNAME\AppData\Roaming\Godot\app_userdata\BasicGameGodot`.

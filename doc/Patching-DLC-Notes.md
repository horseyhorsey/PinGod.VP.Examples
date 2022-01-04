# Game Updates / Patches / DLC / Mods
---

## Patches
---

Dev can supply .pck files named `patch_1.pck`, `patch_2.pck` and so on with the exported game.

These will be loaded from the exported game directory under `patches`. `res://patches/patch_1.pck`

#### Godot notes, guide:

Deliver the project to players then when one wants to add functionality or content later on, just deliver the updates via PCK files to the players.

Instead of shipping whole game over again. .Pcks can be made and imported by the game later on.

Resources like scenes, scripts etc can be exported into pack from the export menu and on the resources tab.

https://docs.godotengine.org/en/stable/getting_started/workflow/export/exporting_pcks.html#use-cases
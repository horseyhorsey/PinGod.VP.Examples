---
title: "BasicGame - Setup / AddOns"
date: 2017-10-17T15:26:15Z
lastmod: 2019-10-26T15:26:15Z
draft: false
weight: 23
---

To run the `BasicGameGodot` project you will need the `addOns` directory linked or copied.

These `addons` contain base files and pinball framework that a game will use.

#### Link the PinGod.VP.AddOns

You can copy the `addons` directory to your project (`BasicGameGodot` in this example), but it's better to use a `symbolic link` to the addons.

The `_create_addons_link.bat` file that you can use in the projects root directory can do this for you when running it, see image:

![image](../../images/basicgame-bats.jpg)

A shortcut `addons` folder will be added in the Godot project.

![image](../../images/basicgame-project-files.jpg)
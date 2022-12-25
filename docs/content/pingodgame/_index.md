---
title: "PinGodGame (AutoLoaded)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 20
---

{{% panel status="primary" title="Note" icon="far fa-lightbulb" %}}
This scene is autoloaded with a game and you should add this scene as a favorite in Godots favorites scene tree for quicker access.

{{% /panel %}}

Most modes rely on this being loaded by using `GetNode<PinGodGame>("/root/PinGodGame")` from the tree

Scene inspector uses `[export]`s that are defined in `game\PinGodGame.cs`. You can add your own to your game from inheriting from the PinGodGame

![image](../../images/godot-pingodgame-tscn.jpg)
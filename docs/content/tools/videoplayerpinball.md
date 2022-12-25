---
title: "VideoPlayerPinball (VideoPlayer)"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

## VideoPlayerPinball (AddOn)
---

This helper node for video has options to set looping and other in the inspector when added to the tree.

Tool: This can be added with `Create New Node` and searching in godot editor for control `VideoPlayerPinball`

### Adding VideoPlayerPinball to Attract Mode

#### 1. Using a duplicated BasicGame as starter project
---

- Open project with Godot
- Find the `Attract.tscn` in the addons `res://addons/Modes/attract`, right click and duplicate and give it a name, `CustomAttract.tscn`
- Drag the `CustomAttract.tscn` to the modes folder in the game project (or move in file computer system)
- Open `MainScene.tscn` in text editor and update the attract scene to `res://modes/CustomAttract.tscn`. Click Godot to `Reload` the scene.

#### 2. Editing the `CustomAttract` scene
---

- Delete the `Background` in the scene. This will remove the grey background behind the PUSH START text.
- Select the top node, `Attract` then right click > add child Node. Search for `VideoPlayerPinball` and add one. * If this node isn't in the search then the addons aren't enabled.
- Push the `Layout` button whilst selected on the `VideoPlayer` and anchor it to be `Full Rect` to fill the screen
- Add your video file to the players Stream dropdown and use the options in the inspector.
- Check `Play When Visible` and `Loop` and run the game / scene
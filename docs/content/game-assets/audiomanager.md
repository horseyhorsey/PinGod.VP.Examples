---
title: "Assets - Audio Manager"
date: 2017-10-17T15:26:15Z
draft: false
weight: 40
---

Helper class for audio.

note: if you want the finished signal on audio to work when the file is an ogg, the loop must be unchecked then reimported from the import tab

- SfxAssets
- VoiceAssets
- MusicAssets

## _EnterTree

- Initializes the AudioStreamPlayers.
- Loads sound pack resources and adds any assets found into the provided dictionaries.
- Will try and load a `pingod.snd.pck` asset file, tries `res://pingod.snd.pck` first, then the working directory

## Methods

PinGodGame has wrapped some of the common methods so you can use a PinGodGame reference to do audio controls, playing audio, music, voices.
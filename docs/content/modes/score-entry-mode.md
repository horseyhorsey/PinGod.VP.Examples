---
title: "Score Entry"
date: 2017-10-17T15:26:15Z
draft: false
weight: 10
---

Basic Score mode with charachter selection for players at end of game

## Scene Tree breadown

- ColorRect = Black background color
- SelectedChar
- Label = HI_SCORE_ENTRY
- CenterContainer = Shows selected char center screen
- ColorRect2 = used as red marker under a letter

## Exports (ScoreEntry.cs)

- _includeZeroToNine
- _nameMaxLength 
- _playerMessage
_selectCharMargin = space when changing between chars with flippers
_selectedChar

## Overrides

### _Ready

Sets `IsPlayerEnteringScore` to true quits the scene if no players or `MoveNextPlayer`

### _Input

If scene is visible and `IsPlayerEnteringScore` then `flippers` and `start` button actions are watched to then be able to select letter.

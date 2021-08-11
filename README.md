# OpenConstructionSet Patcher for SCAR's pathfinding fix

## Overview
This application uses the [OCS](https://github.com/lmaydev/OpenConstructionSet) to automatically apply key values from SCAR's pathfinding fix and create a compatibility mod.

### Features
 - Detects game's folders and the Steam Workshop content folder.
 - Mod discovery. Works for data, mod and steam folders.
 - Load order is read from the game's config and can be saved back if you make adjusatments.
 - One click mod creation if using standard configuration.

### [SCARaw](https://www.nexusmods.com/kenshi/users/16691049)
Massive thanks to Scar for helping to get this project off the ground and keep it moving.
Also for the OCS project icon we are using.

### Requirements

 - Kenshi and FCS
 - .Net 4.8
 - SCAR's pathfinding fix ([https://www.nexusmods.com/kenshi/mods/602](https://www.nexusmods.com/kenshi/mods/602))

### Installation

 1. Download a release zip file from the [Releases Page](https://github.com/lmaydev/OpenConstructionSet.Patcher.Scar.PathFinding/releases)
 2. Unzip it into the game's folder (the folder with kenshi and FCS exe files)

### Basic Use

 1. Run OCS.Patcher.Scar.PathFinding.exe
 2. Press the "Create Mod" button
 3. Add "Compatibility SCAR's pathfinding fix" (default name) to your load order in the game's launcher (or this application)

By default the game's data and mods folders will be added. Also the Steam Workshop content folder if found.
All mods from those folders will be discovered.

Your current load order will be read from the game. Mods in your load order will be added in order and selected to be patched.
All other mods will be added afterwards but not selected.

This means if your load order is correct and use standard folders all you need to do is press the "Create Mod" button.
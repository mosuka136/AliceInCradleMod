# BetterExperience/更好的体验

[![GitHub all releases](https://img.shields.io/github/downloads/mosuka136/AliceInCradleMod/total)](https://github.com/mosuka136/AliceInCradleMod/releases) [![GitHub release (latest by date)](https://img.shields.io/github/v/release/mosuka136/AliceInCradleMod)](https://github.com/mosuka136/AliceInCradleMod/releases) ![platform](https://img.shields.io/badge/platform-Windows-lightgrey)

中文版本: [README.md](README.md)

## Overview

`BetterExperience` is an `Alice In Cradle` mod built on top of `BepInEx` and `Harmony`.

This mod uses `Harmony` patches to modify game logic at runtime, providing configurable QoL improvements, stat tweaks, limit removals, convenience features, a built-in visual config UI, and texture replacement support. Most features can be enabled or disabled through the config file or the in-game config UI.

## Implemented Features

- Core controls: mod master switch, standalone config file, built-in config UI, Chinese/English localization, config hot reload, log level control
- QoL features: one-key shop refresh, improved save points, access warehouse anywhere, improved fishing, reel-related adjustments
- Stat tweaks: HP/MP/EP, currency amount, max satiety, movement speed, drop multiplier, cane attributes, danger level, etc.
- Capacity tweaks: backpack capacity, empty bottle holder slots, enhancer slot count, overcharge slot count, etc.
- Survival/combat protection: no HP/MP/EP damage, no map damage, abnormal status immunity, infinite shield, cannot be attacked, etc.
- Trap/environment: drowning, crush damage, falling, MP break, worm traps, and fog visual effects can be enabled or disabled through config
- Limit removals: puppet merchant spawn limits, bench menu limits, treasure chest limits, warehouse region limits, etc.
- Map and weather: fast travel anywhere, dark area removal in specific zones, forced weather (wind/thunder/mist/drought/dense mist/plague)
- Visual features: remove mosaic, texture replacement, sensitivity toggle, runtime texture reload
- Hotkey features: supports key combinations, multiple alternative hotkeys, and gamepad input

## Usage

### 1. Prerequisites

- `Alice In Cradle` installed ([download](https://get.aliceincradle.dev/win/))
- `BepInEx` installed correctly ([download](https://github.com/BepInEx/BepInEx/releases))

### 2. Install BepInEx

1. Go to the `BepInEx` releases page and download the Windows `BepInEx 5` package (usually `x64`).
2. Extract the package and copy all files into the `Alice In Cradle` game root directory (same level as the game `.exe`).
3. Launch the game once and wait for `BepInEx` initialization.
4. Close the game.

### 3. Install BetterExperience

1. Build or download `BetterExperience.dll` ([download](https://github.com/mosuka136/AliceInCradleMod/releases)).
2. Put `BetterExperience.dll` into `BepInEx/plugins/BetterExperience/` (or `BepInEx/plugins/`) under the game directory.
3. Launch the game once to generate the mod config file, log directory, and texture directory.
4. Edit `BepInEx/plugins/BetterExperience/BetterExperience.cfg`, or press `F1` in game to open the built-in config UI.
5. To apply manual config-file changes during gameplay, save the file first, then press your configured `Reload Config` hotkey in game; the default is `Ctrl+R`.
6. To use texture replacement, put `.png` or `.btep` files into `BepInEx/plugins/BetterExperience/ReplaceTexture/`; put sensitive textures into `BepInEx/plugins/BetterExperience/ReplaceTexture/Sensitive/`; press `Ctrl+T` in game to reload textures.

## Supported Versions

- `Alice In Cradle`: `ver029j2`
- `BepInEx`: `v5.4.23.5`

## License

`BetterExperience` is licensed under `LGPL-3.0`. See `LICENSE.txt` for details.

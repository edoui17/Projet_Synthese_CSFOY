# Volume Settings Documentation

## Overview
This document covers the implementation details for the Master, Music, and SFX volume controls located in the Options menu.

## Issue Identified
Previously, only the Music volume slider effectively adjusted audio in-game. The Master and SFX sliders were visible in the UI and their values were persisted to the user's settings file, but changes did not immediately take effect while the game was running.

This occurred because the `value_changed` signals for the Master and SFX sliders in `OptionsMenu.tscn` were mistakenly wired to the root `OptionsMenuManager` node, instead of the `AudioOptions` node where the actual signal handling logic resided. Additionally, there was a misconfigured signal on the Return button.

## Resolution
To ensure a robust, codebase-driven approach and mitigate future Godot UI miswiring:
1. **Programmatic Event Subscription**: Instead of relying entirely on Godot Editor signal configurations in the `.tscn` file, `AudioOptions.cs` was updated. In the `_Ready()` method, the code now explicitly binds to the `ValueChanged` event of all three sliders (`masterSlider`, `musicSlider`, `sfxSlider`).
2. **Method Renaming**: The previously snake_cased Godot default signal names (e.g. `_on_master_soudn_h_slider_value_changed`) were refactored to conform to standard C# PascalCase naming conventions (e.g. `OnMasterSliderValueChanged`).
3. **Save System Integration**: The system still utilizes the existing `DragEnded` event binding to execute `SaveSettings()`. This ensures that disk I/O only occurs when the user finishes adjusting a slider, while the in-game volume updates continuously as the slider is dragged via `ValueChanged`.
4. **Editor Cleanup**: The `.tscn` file was updated to remove the misconfigured signals directly, ensuring a clean and correct state for the Godot Editor without manual intervention.

## Jules AI Usage
Jules, the autonomous AI assistant, was utilized to diagnose the disconnected signals, rewrite the C# signal bindings in `AudioOptions.cs` to ensure they were correctly targeted, fixed the `.tscn` file directly, and generated this documentation.

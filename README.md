# Astra's Menu Template

A customizable Gorilla Tag PCVR mod menu template built using **C# and BepInEx**.

This project provides a complete menu framework for developers who want to create their own mods without having to build the entire menu system from scratch.

## Features

- **3D Hand Menu** — A VR menu with button interaction, categories, and pagination.
- **Automatic Button Registration** — Add buttons using `[MenuButton]` attributes.
- **Custom Categories** — Create categories simply by naming them.
- **Button Types** — Toggle, Press, and Option.
- **Execution Modes** — Update, FixedUpdate, LateUpdate, and Once.
- **Lifecycle Callbacks** — `[MenuEnable]` and `[MenuDisable]`.
- **InputLib** — Controller buttons, triggers, grips, and joystick input.
- **Notification System** — On-screen messages and notifications.
- **Sound System** — Audio playback and sound file management.
- **Custom GUI Support** — IMGUI windows using GlobalStyles and MenuHelper.

## Requirements

- Gorilla Tag PCVR
- BepInEx 5
- Visual Studio 2022 or another compatible C# IDE
- Required Unity, Gorilla Tag, Photon, and SteamVR references

## Creating Buttons

Create a new C# class inside the `Mods` folder.

Use the `[MenuButton]` attribute above any static, parameterless `void` method.

**Button types:**

- `ButtonType.Press` — Executes when clicked.
- `ButtonType.Toggle` — Can be enabled or disabled.
- `ButtonType.Option` — Cycles through a list of options.

**Toggle execution modes:**

- `RunOption.Update` — Runs every frame.
- `RunOption.FixedUpdate` — Runs during physics updates.
- `RunOption.LateUpdate` — Runs after Update.
- `RunOption.Once` — Runs once when enabled.

Use `[MenuEnable("Button Name")]` and `[MenuDisable("Button Name")]` to create setup and cleanup methods.

## Creating Categories

To make a new category, simply set the Category property of a button.

For example:

`Category = "Test"`

The framework automatically discovers the category and adds it to the menu.

No manual category registration is required.

## Included Examples

The `Mods/Test/Test.cs` script contains 13 test buttons demonstrating the framework.

These examples cover:

- Button creation
- Toggle states
- Input detection
- Notifications
- Audio playback
- Sound file discovery
- Custom GUI windows
- Button managers
- Enable and disable callbacks

The code contains comments explaining how each feature works.

For the example IMGUI window, call `Astras_Menu_Template.Mods.Test.Test.GUIWindow()` from your existing `Main.OnGUI()`.

## Customization

You can modify the menu's appearance, controls, categories, and behavior.

The template is intended to be extended with your own C# scripts while keeping the existing menu framework.

## Notes

This project provides source code, not a precompiled mod.

Ensure your development references match the version of Gorilla Tag you are targeting. Test new mods in an appropriate modding environment.

## Credits

Created by **Astra**.

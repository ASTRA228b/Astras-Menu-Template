# Astra's Menu Template

A customizable Gorilla Tag PCVR mod menu template built using **C# and BepInEx**.

**Version 0.0.3 — made by Astra.**

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
- **Eight Themes** — Studio, Coast, Ember, Sakura, Glacier, Terminal, Paper, and Original.
- **Button Styles** — Filled, Outline, and Text Only, with matching GUI colors.
- **Controller Hints** — Add `Controls = "(A)"` or `Controls = "(LG)(RG)"` without changing button names or callbacks.
- **Appearance Preferences** — Theme and button style save automatically between sessions.
- **Menu Layouts** — Standard (5), Compact (7), Large (3), or Grid (8 buttons in two columns).
- **Search** — Find buttons by name, category, description, controls, or option values; type on desktop or use the VR keyboard.

## Menu Design and Controls

The menu uses a centered title and FPS header, a slim accent frame, a search field, category labels, a page indicator, and a consistent Home footer. The header stays clear of logos, theme badges, and version labels. Eight palettes and four layout choices give it its own appearance while following ChudOS's clean visual approach.

- **Desktop:** press **H**, then click with the mouse.
- **VR:** press the left controller's secondary button (**Y** on Quest-style controllers), then use the right-hand pointer.
- **Appearance:** open **Settings → Menu Theme** or **Button Style**. Use **Reset Appearance** to restore Studio with filled buttons.
- **Layout:** open **Settings → Menu Layout**. Options paginate according to the selected layout; Grid uses two columns and wraps long labels.
- **Search:** click the search field. Desktop typing supports Unicode characters; H, Q, and E type normally while editing. Enter/Escape finishes editing, Backspace deletes, and Home clears the search. VR uses the side keyboard's letters, numbers, Space, Del, Clear, and Done keys.

Appearance is stored in `Gorilla Tag/Astra's Menu Template/appearance.json`. Palette, style, and layout names are saved instead of numeric positions. Existing files without a layout retain their theme/style and use Standard. Reset Appearance restores Studio, Filled, and Standard. The template keeps its existing soundboard, notification system, and input library.

To customize the palettes, edit `Mods/Settings/MenuThemes.cs`. The selected palette colors the 3D menu and the included example/soundboard GUI controls.

## Requirements

- Gorilla Tag PCVR
- BepInEx 5
- Visual Studio 2022 or another compatible C# IDE
- Required Unity, Gorilla Tag, Photon, and SteamVR references

## Opening and Building

Open `Astra's Menu Template.sln` and build as usual. The project detects Gorilla Tag from Steam's installation records or `steamapps/libraryfolders.vdf`, including libraries on other drives and Steam installed in a custom folder. You do not need to replace drive letters or re-add DLL references.

The existing build settings are unchanged: the DLL still builds directly into the detected game's `BepInEx/plugins/MO` folder. Debug/Release behavior, target framework, assembly references, and dependency copy settings stay the same.

Gorilla Tag and BepInEx 5 must already be installed. If your installation is not registered with Steam, create `GamePath.user.props` beside `Astras Menu Template.csproj`:

```xml
<Project>
  <PropertyGroup>
    <GorillaTagPath>E:\My Games\Gorilla Tag</GorillaTagPath>
  </PropertyGroup>
</Project>
```

Set this to your game's root folder, containing `Gorilla Tag_Data` and `BepInEx`. This local file is ignored by Git. You can also pass `-p:GorillaTagPath="E:\My Games\Gorilla Tag"` to MSBuild or `dotnet build`. For a custom Steam client folder, the optional `SteamPath` property controls the `libraryfolders.vdf` fallback.

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

### Showing Controller Hints

`Controls` is display metadata; the method still reads input through `InputLib`. For example, the included Trigger Test declares:

```csharp
[MenuButton("Trigger Test", ButtonType.Toggle, Category = "Test", Controls = "(RT)", runOption = RunOption.Update, Description = "Detect right trigger presses", Notify = false)]
public static void TriggerTest()
{
    if (ControllerInputPoller.instance == null) return;
    bool trigger = InputLib.RightTrigger;
    if (trigger && !LastTrigger) CanvasNotifyLib.SendCanvasNotify("Right Trigger pressed!");
    LastTrigger = trigger;
}
```

The example class owns the `LastTrigger` field and resets it in its lifecycle callbacks. The menu displays `Trigger Test (RT) [ON/OFF]`, while its registered name remains `Trigger Test`.

Hints: `(A)`, `(B)`, `(X)`, `(Y)` are face buttons; `(LT)`/`(RT)` are triggers; `(LG)`/`(RG)` are grips; `(LJ)`/`(RJ)` are joystick clicks. Describe joystick movement as an axis in the description so it is not confused with a click.

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

The example IMGUI window is already connected to `Main.OnGUI()`; use its example button to open it.

## Customization

You can modify the menu's appearance, controls, categories, and behavior.

The template is intended to be extended with your own C# scripts while keeping the existing menu framework.

## Notes

The source archive is uploaded as a named release asset, `Astras.Menu.Template.zip`, so GitHub can track its asset download count separately from automatic source archives. It includes the solution, project, source, and portable Steam-path configuration. IDE caches, build intermediates, local path overrides, and game DLLs are excluded.

If a compiled test DLL is provided with a release, install only that plugin DLL into `BepInEx/plugins`; the source ZIP is for editing and rebuilding.

Ensure your development references match the version of Gorilla Tag you are targeting. Test new mods in an appropriate modding environment.

## Credits

Created by **Astra**.

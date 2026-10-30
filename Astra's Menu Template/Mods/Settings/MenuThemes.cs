using Astras_Menu_Template.Core;
using Astras_Menu_Template.Core.Managers;
using Astras_Menu_Template.Stuff;
using UnityEngine;

namespace Astras_Menu_Template.Mods.Settings;

public static class MenuThemes
{
    public sealed class Palette
    {
        public readonly string Name;
        public readonly Color Panel, Button, Accent, Text, Muted;
        public Color OnAccent => Accent.grayscale > 0.55f ? new Color32(15, 20, 30, 255) : Color.white;
        public Palette(string name, Color panel, Color button, Color accent, Color text, Color muted)
        {
            Name = name; Panel = panel; Button = button; Accent = accent; Text = text; Muted = muted;
        }
    }
    [Serializable]
    private sealed class Appearance
    {
        public string Theme = "Studio";
        public string Buttons = "Filled";
        public string Layout = "Standard";
    }
    private static Color RGB(byte r, byte g, byte b) => new Color32(r, g, b, 255);
    private static readonly Palette[] Palettes =
    {
        new("Studio", RGB(17, 20, 32), RGB(31, 36, 53), RGB(170, 149, 255), RGB(242, 242, 250), RGB(155, 164, 189)),
        new("Coast", RGB(10, 25, 33), RGB(18, 43, 53), RGB(76, 223, 196), RGB(233, 250, 247), RGB(132, 178, 184)),
        new("Ember", RGB(28, 22, 22), RGB(48, 35, 33), RGB(255, 157, 91), RGB(255, 243, 230), RGB(190, 156, 141)),
        new("Sakura", RGB(29, 20, 33), RGB(49, 33, 53), RGB(244, 152, 196), RGB(255, 239, 249), RGB(192, 152, 179)),
        new("Glacier", RGB(15, 25, 39), RGB(26, 43, 62), RGB(131, 204, 255), RGB(237, 247, 255), RGB(141, 173, 199)),
        new("Terminal", RGB(10, 19, 15), RGB(19, 36, 27), RGB(135, 233, 155), RGB(224, 248, 226), RGB(129, 169, 139)),
        new("Paper", RGB(236, 238, 244), RGB(219, 224, 235), RGB(68, 92, 173), RGB(29, 37, 57), RGB(87, 100, 128)),
        new("Original", RGB(14, 11, 22), RGB(31, 23, 48), RGB(115, 51, 217), Color.white, RGB(183, 165, 210))
    };
    private static readonly string[] ButtonStyles = { "Filled", "Outline", "Text Only" };
    private static readonly string[] Layouts = { "Standard", "Compact", "Large", "Grid" };
    private static int selected;
    public static Palette Current => Palettes[selected];
    public static int ButtonStyle { get; private set; }
    public static int Layout { get; private set; }
    private static string PreferencesPath => Path.Combine(FileManager.RootFolder, "appearance.json");

    internal static void Initialize()
    {
        selected = ButtonStyle = Layout = 0;
        try
        {
            if (File.Exists(PreferencesPath) && new FileInfo(PreferencesPath).Length <= 16384)
            {
                Appearance? saved = JsonUtility.FromJson<Appearance>(File.ReadAllText(PreferencesPath));
                if (saved != null)
                {
                    selected = Mathf.Max(0, Array.FindIndex(Palettes, p => p.Name == saved.Theme));
                    ButtonStyle = Mathf.Max(0, Array.IndexOf(ButtonStyles, saved.Buttons));
                    Layout = Mathf.Max(0, Array.IndexOf(Layouts, saved.Layout));
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning($"[{Constantss.GUID}] Appearance settings: {ex.Message}"); }
        var theme = ButtonManager.GetButton("Menu Theme");
        var buttons = ButtonManager.GetButton("Button Style");
        if (theme != null) theme.OptionIndex = selected;
        if (buttons != null) buttons.OptionIndex = ButtonStyle;
        var layout = ButtonManager.GetButton("Menu Layout");
        if (layout != null) layout.OptionIndex = Layout;
        ApplyLayout();
        Main.ApplyTheme(Current);
    }

    [MenuButton("Menu Theme", ButtonType.Option, Category = "Settings", Order = 10, Options = new[] { "Studio", "Coast", "Ember", "Sakura", "Glacier", "Terminal", "Paper", "Original" }, Notify = false)]
    public static void SetTheme()
    {
        selected = Mathf.Clamp(ButtonManager.GetButton("Menu Theme")?.OptionIndex ?? 0, 0, Palettes.Length - 1);
        Main.ApplyTheme(Current);
        Save();
    }
    [MenuButton("Button Style", ButtonType.Option, Category = "Settings", Order = 11, Options = new[] { "Filled", "Outline", "Text Only" }, Notify = false)]
    public static void SetButtonStyle()
    {
        ButtonStyle = Mathf.Clamp(ButtonManager.GetButton("Button Style")?.OptionIndex ?? 0, 0, ButtonStyles.Length - 1);
        Main.RefreshAppearance();
        Save();
    }
    [MenuButton("Menu Layout", ButtonType.Option, Category = "Settings", Order = 12, Options = new[] { "Standard", "Compact", "Large", "Grid" }, Notify = false)]
    public static void SetLayout()
    {
        Layout = Mathf.Clamp(ButtonManager.GetButton("Menu Layout")?.OptionIndex ?? 0, 0, Layouts.Length - 1);
        ApplyLayout();
        Main.RefreshAppearance();
        Save();
    }
    private static void ApplyLayout()
    {
        Main.ButtonPageLimit = MenuLayout.PageSize(Layout);
        CategoryManager.SetPageLimit(Main.ButtonPageLimit);
    }
    [MenuButton("Reset Appearance", ButtonType.Press, Category = "Settings", Order = 13, Notify = false)]
    public static void Reset()
    {
        var theme = ButtonManager.GetButton("Menu Theme");
        var buttons = ButtonManager.GetButton("Button Style");
        var layout = ButtonManager.GetButton("Menu Layout");
        if (theme != null) ButtonManager.ChangeOption(theme, -theme.OptionIndex);
        if (buttons != null) ButtonManager.ChangeOption(buttons, -buttons.OptionIndex);
        if (layout != null) ButtonManager.ChangeOption(layout, -layout.OptionIndex);
        selected = ButtonStyle = Layout = 0;
        ApplyLayout();
        Main.ApplyTheme(Current);
        Save();
    }
    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(FileManager.RootFolder);
            string temporary = PreferencesPath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(new Appearance { Theme = Current.Name, Buttons = ButtonStyles[ButtonStyle], Layout = Layouts[Layout] }, true), new System.Text.UTF8Encoding(false));
            if (File.Exists(PreferencesPath)) File.Replace(temporary, PreferencesPath, null);
            else File.Move(temporary, PreferencesPath);
        }
        catch (Exception ex) { Debug.LogWarning($"[{Constantss.GUID}] Could not save appearance: {ex.Message}"); }
    }
}

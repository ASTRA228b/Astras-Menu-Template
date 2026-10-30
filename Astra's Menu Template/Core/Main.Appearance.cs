using Astras_Menu_Template.Core.AGUI.GUIHelpers;
using Astras_Menu_Template.Mods.Settings;
using Astras_Menu_Template.Stuff;
using UnityEngine;

namespace Astras_Menu_Template.Core;

public partial class Main
{
    public static Color TextColor = new Color32(242, 242, 250, 255);
    public static Color MutedTextColor = new Color32(155, 164, 189, 255);
    public static Color HeaderColor, ActiveTextColor;
    private static Material? HeaderMaterial, ShadowMaterial;
    private sealed class TextLayout { internal float Width, Height, Maximum; internal int Tone; }
    private static readonly Dictionary<TextMesh, TextLayout> TextLayouts = new();
    private static int fitPasses;

    public static void ApplyTheme(MenuThemes.Palette theme)
    {
        MenuColor = theme.Panel;
        MenuOutlineColor = BottonPressedColor = theme.Accent;
        ButtonColor = theme.Button;
        TextColor = theme.Text;
        MutedTextColor = theme.Muted;
        HeaderColor = Color.Lerp(theme.Panel, theme.Button, 0.65f);
        ActiveTextColor = theme.OnAccent;
        GlobalStyles.Configure(theme.Panel, theme.Button, theme.Accent, theme.Text, ActiveTextColor);
        RefreshAppearance();
    }
    public static void RefreshAppearance()
    {
        if (BackgroundMaterial != null) BackgroundMaterial.color = MenuColor;
        if (OutlineMaterial != null) OutlineMaterial.color = MenuOutlineColor;
        if (ButtonMaterial != null) ButtonMaterial.color = ButtonColor;
        if (HeaderMaterial != null) HeaderMaterial.color = HeaderColor;
        foreach (var entry in TextLayouts)
            if (entry.Key != null) entry.Key.color = TextTone(entry.Value.Tone);
        keyboardDirty = true;
        RefreshLater();
    }
    private static void CreateAppearanceMaterials(Shader shader)
    {
        HeaderMaterial = new Material(shader) { color = HeaderColor };
        ShadowMaterial = new Material(shader) { color = new Color(0.015f, 0.02f, 0.03f) };
    }
    private static void BuildShell()
    {
        Transform root = MenuMain!.transform;
        CreateCube("Panel Shadow", root, new Vector3(0.10f, -0.155f, -0.015f), new Vector3(0.12f, 2.24f, 1.12f), ShadowMaterial);
        CreateOutline(root, new Vector3(0.065f, -0.14f, 0f), new Vector3(0.15f, 2.22f, 1.10f), 0f);
        MenuBackground = CreateCube("Background", root, new Vector3(0f, -0.14f, 0f), new Vector3(0.15f, 2.22f, 1.10f), BackgroundMaterial);
        TextMesh title = CreateText(Constantss.MenuName, 0.76f, 0f, 0.027f);
        RegisterText(title, 0.99f, 0.027f, 0.16f);
        FPS = CreateText("FPS: --", 0.58f, 0f, 0.014f);
        SetTextTone(FPS, 1);
        CreateCube("Footer Divider", root, new Vector3(-0.10f, -0.90f, 0f), new Vector3(0.015f, 0.005f, 0.97f), HeaderMaterial);
    }
    private static void RegisterText(TextMesh text, float width, float maximum, float height = 0.12f)
    {
        if (!TextLayouts.TryGetValue(text, out TextLayout? layout)) TextLayouts.Add(text, layout = new TextLayout());
        layout.Width = width;
        layout.Height = height;
        layout.Maximum = maximum;
        int longest = text.text.Split('\n').Max(line => line.Length);
        text.characterSize = Mathf.Min(maximum, width * 0.88f / Mathf.Max(3f, longest * 3f));
        fitPasses = 2;
    }
    private static void SetTextTone(TextMesh text, int tone)
    {
        if (TextLayouts.TryGetValue(text, out TextLayout? layout)) layout.Tone = tone;
        text.color = TextTone(tone);
    }
    private static Color TextTone(int tone) => tone == 1 ? MutedTextColor : tone == 2 ? ActiveTextColor : tone == 3 ? Color.white : TextColor;
    private static void FitVisibleLabels()
    {
        if (!IsMenuOpen || fitPasses <= 0) return;
        fitPasses--;
        foreach (var entry in TextLayouts)
        {
            TextMesh text = entry.Key;
            if (text == null || !text.gameObject.activeInHierarchy) continue;
            Renderer renderer = text.GetComponent<Renderer>();
            if (renderer == null) continue;
            Vector3 bounds = renderer.localBounds.size;
            if (float.IsNaN(bounds.x) || float.IsInfinity(bounds.x) || float.IsNaN(bounds.y) || float.IsInfinity(bounds.y)) continue;
            float factor = Mathf.Min(1f, Mathf.Min(entry.Value.Width / Mathf.Max(bounds.x, 0.0001f), entry.Value.Height / Mathf.Max(bounds.y, 0.0001f)));
            if (factor < 1f) text.characterSize *= factor * 0.96f;
        }
    }
    public static void UpdateButtonLabel(TextMesh text, string value)
    {
        if (text == null) return;
        if (!TextLayouts.TryGetValue(text, out TextLayout? layout)) { text.text = value; FitButtonText(text, value); return; }
        string formatted = FormatButtonLabel(value, layout.Width);
        if (text.text == formatted) return;
        text.text = formatted;
        RegisterText(text, layout.Width, layout.Maximum, layout.Height);
    }
    private static string FormatButtonLabel(string value, float width)
    {
        if (width >= 0.55f || value.Length <= 18 || value.Contains('\n')) return value;
        int middle = value.Length / 2, split = value.LastIndexOf(' ', middle);
        if (split < 1) split = value.IndexOf(' ', middle);
        return split > 0 && split < value.Length - 1 ? value.Substring(0, split) + "\n" + value.Substring(split + 1) : value;
    }
    private static void StyleButton(GameObject button)
    {
        Renderer renderer = button.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = MenuThemes.ButtonStyle == 0;
        if (MenuThemes.ButtonStyle != 1) return;
        GameObject obj = new("Button Edge");
        obj.transform.SetParent(button.transform, false);
        float thickness = Mathf.Clamp(Outlinethick * 0.3f, 0.003f, 0.012f);
        Vector3 scale = button.transform.localScale;
        float x = OnPC ? -0.51f : 0.51f;
        Vector3 horizontal = new(0.01f / scale.x, thickness / scale.y, 0.96f);
        Vector3 vertical = new(0.01f / scale.x, 0.96f, thickness / scale.z);
        CreateCube("Top", obj.transform, new Vector3(x, 0.48f, 0f), horizontal, OutlineMaterial);
        CreateCube("Bottom", obj.transform, new Vector3(x, -0.48f, 0f), horizontal, OutlineMaterial);
        CreateCube("Left", obj.transform, new Vector3(x, 0f, 0.48f), vertical, OutlineMaterial);
        CreateCube("Right", obj.transform, new Vector3(x, 0f, -0.48f), vertical, OutlineMaterial);
    }
    private static void ReleaseAppearance()
    {
        if (HeaderMaterial != null) Destroy(HeaderMaterial);
        if (ShadowMaterial != null) Destroy(ShadowMaterial);
        HeaderMaterial = ShadowMaterial = null;
        TextLayouts.Clear();
    }
}

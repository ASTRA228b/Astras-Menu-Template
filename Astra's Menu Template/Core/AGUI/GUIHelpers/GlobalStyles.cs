using UnityEngine;

namespace Astras_Menu_Template.Core.AGUI.GUIHelpers;

public static class GlobalStyles
{
    public static GUIStyle? WindowStyle, Buttonss, SliderStyle, SliderThumbStyle, LabelStyle, ToggleStyle;
    private static Texture2D? WindowTex, ButtonTex, SliderTex, ThumbTex;
    private static Color WindowColor = new Color32(17, 20, 32, 255), ButtonColor = new Color32(31, 36, 53, 255), SliderColor = new Color32(25, 30, 45, 255), ThumbColor = new Color32(170, 149, 255, 255), Foreground = Color.white, ActiveForeground = Color.black;

    public static void Configure(Color panel, Color button, Color accent, Color text, Color activeText)
    {
        WindowColor = panel; ButtonColor = button; SliderColor = Color.Lerp(panel, button, 0.6f); ThumbColor = accent;
        Foreground = text; ActiveForeground = activeText;
        Recolor(WindowTex, WindowColor); Recolor(ButtonTex, ButtonColor); Recolor(SliderTex, SliderColor); Recolor(ThumbTex, ThumbColor);
        ApplyColors();
    }
    private static void Recolor(Texture2D? texture, Color color)
    {
        if (texture == null) return;
        texture.SetPixel(0, 0, color); texture.Apply();
    }

    public static void INIT()
    {
        if (WindowStyle != null) return;
        WindowTex = GlobalTex.MakeTex(1, 1, WindowColor);
        ButtonTex = GlobalTex.MakeTex(1, 1, ButtonColor);
        SliderTex = GlobalTex.MakeTex(1, 1, SliderColor);
        ThumbTex = GlobalTex.MakeTex(1, 1, ThumbColor);
        WindowStyle = new GUIStyle(GUI.skin.window);
        Buttonss = new GUIStyle(GUI.skin.button);
        SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        LabelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
        ToggleStyle = new GUIStyle(GUI.skin.toggle);
        ApplyBackground(WindowStyle, WindowTex);
        ApplyBackground(Buttonss, ButtonTex);
        ApplyBackground(SliderStyle, SliderTex);
        ApplyBackground(SliderThumbStyle, ThumbTex);
        Buttonss.active.background = Buttonss.onNormal.background = ThumbTex;
        ApplyColors();
        WindowStyle.padding = new RectOffset(14, 14, 28, 12);
        Buttonss.fixedHeight = 27f;
        Buttonss.wordWrap = true;
        Buttonss.padding = new RectOffset(8, 8, 3, 3);
    }

    private static void ApplyColors()
    {
        foreach (GUIStyle? style in new[] { WindowStyle, Buttonss, LabelStyle, ToggleStyle })
        {
            if (style == null) continue;
            foreach (GUIStyleState state in new[] { style.normal, style.hover, style.active, style.focused, style.onNormal, style.onHover, style.onActive, style.onFocused }) state.textColor = Foreground;
        }
        if (Buttonss != null) Buttonss.active.textColor = Buttonss.onNormal.textColor = ActiveForeground;
    }

    private static void ApplyBackground(GUIStyle style, Texture2D tex)
    {
        style.normal.background = tex;
        style.hover.background = tex;
        style.active.background = tex;
        style.focused.background = tex;
        style.onNormal.background = tex;
        style.onHover.background = tex;
        style.onActive.background = tex;
        style.onFocused.background = tex;
    }

    public static void Cleanup()
    {
        foreach (Texture2D? tex in new[] { WindowTex, ButtonTex, SliderTex, ThumbTex }) if (tex != null) GameObject.Destroy(tex);
        WindowStyle = Buttonss = SliderStyle = SliderThumbStyle = LabelStyle = ToggleStyle = null;
        WindowTex = ButtonTex = SliderTex = ThumbTex = null;
    }
}

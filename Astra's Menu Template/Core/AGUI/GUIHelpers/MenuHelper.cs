using UnityEngine;

namespace Astras_Menu_Template.Core.AGUI.GUIHelpers;

public static class MenuHelper
{
    public static void Label(string text) => GUILayout.Label(text);
    public static bool Button(string text) => GUILayout.Button(text, GlobalStyles.Buttonss);
    public static void Space(float space = 6f) => GUILayout.Space(space);
    public static bool Toggle(string text, bool state) => GUILayout.Toggle(state, text);
    public static float Slider(float current, float min, float max) => GUILayout.HorizontalSlider(current, min, max, GlobalStyles.SliderStyle, GlobalStyles.SliderThumbStyle); // Just tiny GUI helpers, might save some time.
}

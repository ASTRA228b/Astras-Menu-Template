using UnityEngine;
using Astras_Menu_Template.Stuff;
using Astras_Menu_Template.Core.Managers;

namespace Astras_Menu_Template.Core.AGUI;

public static class ArrayListGUI // Turn this off in settings to boost profromance a little bit, fix that issue if you want.
{
    private static GUIStyle? Something;
    public static void CreateArray()
    {
        if (!GUIManager.ArrayListEnabled || Event.current.type != EventType.Repaint) return;
        Something ??= new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
        Something.normal.textColor = new Color(0.82f, 0.64f, 1f);
        string[] enabled = ButtonManager.GetEnabledNames().ToArray();
        GUI.Label(new Rect(Screen.width - 300, 12, 280, 25), $"{Constantss.ArrayListName} | {enabled.Length} ACTIVE", Something);
        for (int i = 0; i < enabled.Length; i++) GUI.Label(new Rect(Screen.width - 300, 41 + i * 21, 280, 22), enabled[i], Something);
    }
}

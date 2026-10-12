using UnityEngine;

namespace Astras_Menu_Template.Core.AGUI.GUIHelpers;

public static class GlobalTex
{
    public static Texture2D MakeTex(int W, int H, Color C)
    {
        Texture2D T = new(W, H);
        T.SetPixel(0, 0, C);
        T.Apply();
        return T;
    }
}

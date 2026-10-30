using Astras_Menu_Template.Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astras_Menu_Template.Core;

public partial class Main
{
    private static readonly string[] KeyboardRows = { "1234567890", "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
    private static readonly List<GameObject> KeyboardObjects = new();
    private static bool keyboardBuilt, keyboardDirty = true, desktopBoundsDirty = true;
    private Keyboard? searchKeyboard;
    private static int desktopWidth = -1, desktopHeight = -1;
    private static float desktopAspect = -1f, desktopProjection = -1f;
    private const float KeyboardCenterZ = -1.22f, KeyboardWidth = 1.14f, KeyboardHeight = 1.28f;

    private void BindSearchKeyboard(Keyboard? keyboard)
    {
        if (searchKeyboard == keyboard) return;
        if (searchKeyboard != null) searchKeyboard.onTextInput -= SearchTextInput;
        searchKeyboard = keyboard;
        if (searchKeyboard != null) searchKeyboard.onTextInput += SearchTextInput;
    }
    private void SearchTextInput(char value)
    {
        if (!IsMenuOpen || !SearchManager.Typing || !Application.isFocused || char.IsControl(value) || char.IsSurrogate(value)) return;
        SearchManager.Key(value.ToString());
    }
    private void UpdateSearchInput(Keyboard? keyboard)
    {
        BindSearchKeyboard(keyboard);
        if (!SearchManager.Typing || keyboard == null || !Application.isFocused) return;
        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame) SearchManager.Blur();
        else if (keyboard.backspaceKey.wasPressedThisFrame) SearchManager.Key("Del");
    }
    private static void RefreshSearchKeyboard()
    {
        if (!SearchManager.Typing || MenuMain == null) { ClearSearchKeyboard(); return; }
        if (keyboardBuilt && !keyboardDirty) return;
        ClearSearchKeyboard();
        keyboardBuilt = true;
        keyboardDirty = false;
        desktopBoundsDirty = true;
        KeyboardObjects.Add(CreateCube("Search Keyboard", MenuMain.transform, new Vector3(0f, -0.10f, KeyboardCenterZ), new Vector3(0.15f, KeyboardHeight, KeyboardWidth), BackgroundMaterial));
        TextMesh heading = CreateText("KEYBOARD", 0.46f, KeyboardCenterZ, 0.012f);
        SetTextTone(heading, 1);
        KeyboardObjects.Add(heading.gameObject);
        for (int row = 0; row < KeyboardRows.Length; row++)
        {
            string keys = KeyboardRows[row];
            float first = KeyboardCenterZ + (keys.Length - 1) * 0.101f / 2f;
            for (int i = 0; i < keys.Length; i++)
                CreateButton(keys[i].ToString(), 0.30f - row * 0.19f, centerZ: first - i * 0.101f, cellWidth: 0.087f, height: 0.16f, key: keys[i].ToString(), keyboard: true);
        }
        CreateButton("Space", -0.56f, centerZ: KeyboardCenterZ + 0.32f, cellWidth: 0.24f, height: 0.155f, key: "Space", keyboard: true);
        CreateButton("Del", -0.56f, centerZ: KeyboardCenterZ + 0.10f, cellWidth: 0.16f, height: 0.155f, key: "Del", keyboard: true);
        CreateButton("Clear", -0.56f, centerZ: KeyboardCenterZ - 0.12f, cellWidth: 0.22f, height: 0.155f, key: "Clear", keyboard: true);
        CreateButton("Done", -0.56f, centerZ: KeyboardCenterZ - 0.36f, cellWidth: 0.22f, height: 0.155f, key: "Done", keyboard: true);
        RefreshDesktopBounds(true);
    }
    private static void ClearSearchKeyboard()
    {
        if (!keyboardBuilt) return;
        foreach (GameObject obj in KeyboardObjects)
        {
            if (obj == null) continue;
            if (obj.TryGetComponent(out TextMesh text)) TextLayouts.Remove(text);
            obj.SetActive(false);
            Destroy(obj);
        }
        KeyboardObjects.Clear();
        keyboardBuilt = false;
        desktopBoundsDirty = true;
    }
    private static void RefreshDesktopBounds(bool force = false)
    {
        if (!OnPC || MenuMain == null || PCamera == null) return;
        float projection = PCamera.orthographic ? PCamera.orthographicSize : PCamera.fieldOfView;
        if (!force && !desktopBoundsDirty && desktopWidth == Screen.width && desktopHeight == Screen.height && desktopAspect == PCamera.aspect && desktopProjection == projection) return;
        desktopBoundsDirty = false;
        desktopWidth = Screen.width; desktopHeight = Screen.height;
        desktopAspect = PCamera.aspect; desktopProjection = projection;
        float minZ = keyboardBuilt ? KeyboardCenterZ - KeyboardWidth / 2f : -0.56f;
        const float maxZ = 0.56f, minY = -1.26f, maxY = 1.15f;
        float halfHeight = PCamera.orthographic ? PCamera.orthographicSize : Mathf.Tan(PCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.8f;
        float factor = Mathf.Min(1f, Mathf.Min(halfHeight * 1.8f / ((maxY - minY) * MenuScale.y), halfHeight * PCamera.aspect * 1.8f / ((maxZ - minZ) * MenuScale.z)));
        if (float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f) return;
        MenuMain.transform.localScale = MenuScale * factor;
        MenuMain.transform.localPosition = new Vector3((minZ + maxZ) * MenuScale.z * factor / 2f, -(minY + maxY) * MenuScale.y * factor / 2f, 0.8f);
    }
}

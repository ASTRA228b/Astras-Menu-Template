using UnityEngine.InputSystem;

namespace Astras_Menu_Template.Core.Managers;

public static class GUIManager
{
    public static bool PCView;
    public static bool ArrayListEnabled = true;
    public static Key GUIInput = Key.Q;
    public static Key CloseAllInput = Key.E;
    public static readonly Key[] Inputs = { Key.Tab, Key.CapsLock, Key.Escape, Key.LeftShift, Key.Q, Key.E };
    private static readonly Dictionary<string, (Action<bool> Set, Func<bool> Get)> Windows = new();
    public static void Register(string name, Action<bool> setter, Func<bool> getter) => Windows[name] = (setter, getter);
    public static void Toggle(string name) { if (Windows.TryGetValue(name, out var window)) window.Set(!window.Get()); }
    public static void CloseAll() { foreach (var window in Windows.Values) window.Set(false); }
    public static void ToggleAll()
    {
        bool anyOpen = false;
        foreach (var window in Windows.Values) if (window.Get()) { anyOpen = true; break; }
        foreach (var window in Windows.Values) window.Set(!anyOpen);
    }
    public static void Tick()
    {
        Keyboard? keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard[CloseAllInput].wasPressedThisFrame) CloseAll();
        else if (GUIInput != CloseAllInput && keyboard[GUIInput].wasPressedThisFrame) ToggleAll();
    }
    public static void SetOpenKey(int index) => GUIInput = Inputs[UnityEngine.Mathf.Clamp(index, 0, Inputs.Length - 1)];
    public static void SetCloseKey(int index) => CloseAllInput = Inputs[UnityEngine.Mathf.Clamp(index, 0, Inputs.Length - 1)];
}

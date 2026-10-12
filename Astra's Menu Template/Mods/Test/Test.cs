using Astras_Menu_Template.Core.AGUI.GUIHelpers;
using Astras_Menu_Template.Core.Managers;
using Astras_Menu_Template.Libraries;
using Astras_Menu_Template.Libraries.ChudOS;
using Astras_Menu_Template.Libraries.ChudOS.CanvasNotify;
using Astras_Menu_Template.Stuff;
using UnityEngine;

namespace Astras_Menu_Template.Mods.Test;

// ButtonManager finds methods with [MenuButton] automatically at startup.
// Button names must be unique throughout the menu, even across categories.
// Every attributed method must be static, return void, and take no arguments.
public static class Test
{
    private static int Counter;
    private static bool ShowExtra = true;
    private static float TestVolume = 0.5f;
    private static bool LastTrigger, LastGrip;
    private static float UpdateTime;
    private static Vector3 LastLookDirection;
    private static AudioClip? BeepClip;

    private static bool WindowVisible, WindowRegistered;
    private static Rect Window = new(200f, 160f, 330f, 250f);

    // PRESS: Run this method once each time the button is clicked.
    // Category = "Test" adds the category automatically; Order controls position.
    // Notify = false stops the framework's automatic toast because we send our own below.
    [MenuButton("Hello World", ButtonType.Press, Category = "Test", Order = 0, Description = "A basic press button", Notify = false)]
    public static void HelloWorld()
    {
        Debug.Log("[Astra Template] Hello from a menu button!");
        NotifiLib.SendNotificationALL("Hello from Astra's Menu Template!", 3000);
    }

    // PRESS: Store simple state in a static field.
    [MenuButton("Add To Counter", ButtonType.Press, Category = "Test", Order = 1, Description = "Counts button presses", Notify = false)]
    public static void AddToCounter()
    {
        Counter++;
        CanvasNotifyLib.SendCanvasNotify($"Counter: {Counter}");
    }

    // TOGGLE + UPDATE: This method executes each Update while the button is ON.
    // InputLib exposes controller buttons and joystick axes.
    [MenuButton("Trigger Test", ButtonType.Toggle, Category = "Test", Order = 2, runOption = RunOption.Update, Description = "Detect right trigger presses", Notify = false)]
    public static void TriggerTest()
    {
        if (ControllerInputPoller.instance == null) return;
        bool trigger = InputLib.RightTrigger;

        // Edge detection: a message is sent once per press, not every frame.
        if (trigger && !LastTrigger)
            CanvasNotifyLib.SendCanvasNotify("Right Trigger pressed!");

        LastTrigger = trigger;
        UpdateTime += Time.deltaTime;
    }

    // MENU ENABLE / DISABLE: These callbacks run once when the toggle changes.
    // The string must exactly match the [MenuButton] name.
    [MenuEnable("Trigger Test")]
    public static void EnableTriggerTest()
    {
        LastTrigger = false;
        UpdateTime = 0f;
        NotifiLib.MessageToast("Trigger Test enabled", 2500);
    }

    [MenuDisable("Trigger Test")]
    public static void DisableTriggerTest()
    {
        LastTrigger = false;
        NotifiLib.MessageToast($"Trigger Test ran for {UpdateTime:F1}s", 3000);
        UpdateTime = 0f;
    }

    // OPTION: A click advances through the strings provided in Options.
    // ButtonManager.GetButton(name).Option / OptionIndex read the current selection.
    [MenuButton("Test Mode", ButtonType.Option, Category = "Test", Order = 3, Options = new[] { "Normal", "Fast", "Strong" }, Description = "Cycles through modes", Notify = false)]
    public static void TestMode()
    {
        string mode = ButtonManager.GetButton("Test Mode")?.Option ?? "Unknown";
        NotifiLib.MessageToast($"Mode: {mode}", 2500);
    }

    // ONCE: A toggle whose action runs once when enabled, rather than each frame.
    // It remains marked ON until toggled off; use a Press button for momentary actions.
    [MenuButton("Run Once", ButtonType.Toggle, Category = "Test", Order = 4, runOption = RunOption.Once, Description = "Runs on enable only", messageEnable = "Run Once enabled", messageDisable = "Run Once disabled")]
    public static void RunOnce() => Debug.Log("[Astra Template] Run Once was enabled.");

    // FIXED UPDATE: Use FixedUpdate for physics-related updates.
    [MenuButton("Grip Test", ButtonType.Toggle, Category = "Test", Order = 5, runOption = RunOption.FixedUpdate, Description = "Detect left grip presses", Notify = false)]
    public static void GripTest()
    {
        if (ControllerInputPoller.instance == null) return;
        bool grip = InputLib.LeftGrab;
        if (grip && !LastGrip) CanvasNotifyLib.SendCanvasNotify("Left Grip pressed!");
        LastGrip = grip;
    }

    [MenuDisable("Grip Test")]
    public static void DisableGripTest() => LastGrip = false;

    // LATE UPDATE: Runs after Update while enabled. Useful for visual work.
    // This example only reads the camera orientation; it changes nothing in-game.
    [MenuButton("Camera Direction", ButtonType.Toggle, Category = "Test", Order = 6, runOption = RunOption.LateUpdate, Description = "Reads the camera direction", Notify = false)]
    public static void CameraDirection()
    {
        if (Camera.main != null) LastLookDirection = Camera.main.transform.forward;
    }

    // PRESS: See the value collected by the LateUpdate example.
    [MenuButton("Show Direction", ButtonType.Press, Category = "Test", Order = 7, Description = "Print saved camera direction", Notify = false)]
    public static void ShowDirection() => NotifiLib.MessageToast($"Direction: {LastLookDirection:F2}", 3000);

    // INPUT LIBRARY: Reads joystick axes and controller buttons without game modifications.
    [MenuButton("Show Inputs", ButtonType.Press, Category = "Test", Order = 8, Description = "Shows joystick and trigger input", Notify = false)]
    public static void ShowInputs()
    {
        if (ControllerInputPoller.instance == null) return;
        Vector2 stick = InputLib.LeftJoyStickAxis;
        NotifiLib.MessageToast($"Left stick: {stick.x:F2}, {stick.y:F2} | RT: {InputLib.RightTrigger}", 3500);
    }

    // FILE MANAGER: Reads the sound list; GetSounds does not create files.
    [MenuButton("Count Sounds", ButtonType.Press, Category = "Test", Order = 9, Description = "Checks the configured sound folder", Notify = false)]
    public static void CountSounds() => NotifiLib.MessageToast($"Sounds found: {FileManager.GetSounds().Count}", 3000);

    // SOUND MANAGER: Create a short audio clip and use the configured playback output.
    // Cache the clip instead of creating a new one every time the button is clicked.
    [MenuButton("Play Test Beep", ButtonType.Press, Category = "Test", Order = 10, Description = "Plays a short locally generated beep")]
    public static void PlayTestBeep()
    {
        if (BeepClip == null)
        {
            const int rate = 22050;
            float[] samples = new float[rate / 5];
            for (int i = 0; i < samples.Length; i++)
            {
                float fade = 1f - (float)i / samples.Length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * 440f * i / rate) * 0.15f * fade;
            }
            BeepClip = AudioClip.Create("Astra Test Beep", samples.Length, 1, rate, false);
            BeepClip.SetData(samples, 0);
        }
        SoundManager.SetVolume(TestVolume);
        SoundManager.Play(BeepClip);
    }

    [MenuButton("Stop Test Sound", ButtonType.Press, Category = "Test", Order = 11, Description = "Stops SoundManager playback")]
    public static void StopTestSound() => SoundManager.Stop();

    // GUI MANAGER: Register an IMGUI window on first use and toggle it by name.
    // IMPORTANT: This class's GUIWindow() must also be called from Main.OnGUI().
    [MenuButton("Test GUI", ButtonType.Press, Category = "Test", Order = 12, Description = "Opens an IMGUI example")]
    public static void OpenTestGUI()
    {
        if (!WindowRegistered)
        {
            GUIManager.Register("Test GUI", state => WindowVisible = state, () => WindowVisible);
            WindowRegistered = true;
        }
        GUIManager.Toggle("Test GUI");
    }

    // Call this every OnGUI event in your main MonoBehaviour.
    // The styles helper initializes its textures once, not every frame.
    public static void GUIWindow()
    {
        if (!WindowVisible) return;
        GlobalStyles.INIT();
        Window = GUILayout.Window(194826, Window, WindowContent, "Astra's Test GUI", GlobalStyles.WindowStyle);
    }

    private static void WindowContent(int id)
    {
        MenuHelper.Label("This is a custom IMGUI window.");
        MenuHelper.Label($"Counter: {Counter}");
        ShowExtra = MenuHelper.Toggle("Show extra controls", ShowExtra);
        if (ShowExtra)
        {
            MenuHelper.Label($"Test sound volume: {TestVolume:P0}");
            TestVolume = MenuHelper.Slider(TestVolume, 0f, 1f);
            SoundManager.SetVolume(TestVolume);
        }
        MenuHelper.Space();
        if (MenuHelper.Button("Add To Counter")) AddToCounter();
        if (MenuHelper.Button("Close")) WindowVisible = false;
        GUI.DragWindow();
    }
}

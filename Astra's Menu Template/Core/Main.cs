using System;
using System.Collections;
using System.Collections.Generic;
using Astras_Menu_Template.Core.AGUI;
using Astras_Menu_Template.Core.AGUI.GUIHelpers;
using Astras_Menu_Template.Core.Managers;
using Astras_Menu_Template.Libraries;
using Astras_Menu_Template.Libraries.ChudOS;
using Astras_Menu_Template.Libraries.ChudOS.CanvasNotify;
using Astras_Menu_Template.Mods.SoundBoard;
using Astras_Menu_Template.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astras_Menu_Template.Core; // Astras_Menu_Template

public class Main : MonoBehaviour
{
    // Other
    public static bool IsMenuOpen;
    public static TextMesh? FPS;

    // Important
    public static GameObject? MenuCanvas;
    public static GameObject? MenuBackground;
    public static GameObject? MenuMain;
    private static readonly Vector3 MenuScale = new(0.1f, 0.3f, 0.3825f);
    private static readonly Vector3 VRMenuScale = new(0.129f, 0.221f, 0.297f);
    // Attached to the left hand. Rotation stays relative to the wrist.
    public static Vector3 VRMenuPosition = new(0f, 0.02f, 0f);
    public static Vector3 VRMenuRotation = new(0f, -12f, 0f);
    private const float VRMenuFaceOffset = 0.5f;

    public static Color MenuColor = new(0.055f, 0.045f, 0.085f, 1f);
    public static Color MenuOutlineColor = new(0.45f, 0.20f, 0.85f, 1f);
    public static Color ButtonColor = new(0.12f, 0.09f, 0.19f, 1f);
    public static Color BottonPressedColor = new(0.40f, 0.20f, 0.75f, 1f);

    // Data
    public static int PageNumber;
    public static int ButtonPageLimit = 5;
    public static float Outlinethick = 0.02f;

    private static Main? Instance;
    private static GameObject? RightReference;
    private static SphereCollider? RightButtonCollider;
    private static readonly List<GameObject> PageObjects = new();
    private static readonly List<MenuButtonCollider> PageButtons = new();
    private static readonly List<(ButtonManager.ButtonData Button, TextMesh Text)> OptionLabels = new();
    private static Material? BackgroundMaterial, OutlineMaterial, ButtonMaterial;
    private static bool NeedsRefresh, LastController, OnPC;
    private static float LastFPS;
    private static int SmoothedFPS;
    private static Camera? PCamera;
    private static GameObject? VersionObject;
   // private static Transform? PreviousCameraParent;
   // private static Vector3 PreviousCameraPosition;
   // private static Quaternion PreviousCameraRotation;
    private static bool Pending;

    private void Awake()
    {
        Instance = this;
        FileManager.Initialize(this);
        ButtonManager.Initialize();
        CategoryManager.PageLimit = ButtonPageLimit;
        CategoryManager.Initialize();
        SoundboardManager.Initialize(this);
        GUIManager.Register("Soundboard", state => SoundboardGUI.Visible = state, () => SoundboardGUI.Visible);
        ButtonManager.OnNotification += Notify;
        CategoryManager.OnPageChanged += RefreshLater;
        ButtonManager.OnButtonsChanged += RefreshColors;
    }

    private void Start() { CanvasNotifyLib.InitCanvas(); }
    private void OnGUI()
    {
        NotifiLib.InitToast();
        ArrayListGUI.CreateArray();
        SoundboardGUI.GUIWindow();
    }
    private void Update()
    {
        ButtonManager.Update();
        SoundboardManager.Tick();
        GUIManager.Tick();
        CanvasNotifyLib.CanvasUpdate();
        Keyboard? keyboard = Keyboard.current;
        bool vr = ControllerInputPoller.instance != null && ControllerInputPoller.instance.leftControllerSecondaryButton;
        if (vr && !LastController)
        {
            if (IsMenuOpen && !OnPC) RemoveMenu();
            else
            {
                if (IsMenuOpen) RemoveMenu();
                OnPC = false;
                ManageEnableMenu();
            }
        }
        LastController = vr;
        if (keyboard?.hKey.wasPressedThisFrame == true)
        {
            if (IsMenuOpen && OnPC) RemoveMenu();
            else
            {
                if (IsMenuOpen) RemoveMenu();
                OnPC = true;
                ManageEnableMenu();
            }
        }
        if (!IsMenuOpen) return;
        if (OnPC && MenuMain != null)
        {
            if (PCamera != null)
            {
                MenuMain.transform.SetParent(PCamera.transform, false);
                MenuMain.transform.localPosition = new Vector3(-0.12f, -0.02f, 0.80f);
                MenuMain.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                if (Mouse.current?.leftButton.wasPressedThisFrame == true)
                {
                    Ray ray = PCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 2f, 1 << 2, QueryTriggerInteraction.Collide) && hit.collider.TryGetComponent(out MenuButtonCollider button)) button.Press();
                }
            }
        }
        if (NeedsRefresh) { NeedsRefresh = false; RefreshPage(); }
        if (Time.unscaledTime - LastFPS > 0.25f)
        {
            LastFPS = Time.unscaledTime;
            SmoothedFPS = Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime));
            if (FPS != null) FPS.text = $"FPS: {SmoothedFPS}";
            RefreshLabels();
        }
    }
    private void FixedUpdate() { ButtonManager.FixedUpdate(); }
    private void LateUpdate() { ButtonManager.LateUpdate(); }
    private void OnDestroy()
    {
        ButtonManager.OnNotification -= Notify;
        CategoryManager.OnPageChanged -= RefreshLater;
        ButtonManager.OnButtonsChanged -= RefreshColors;
        RemoveMenu();
        SoundboardManager.Shutdown();
        CanvasNotifyLib.Shutdown();
        GunLib.Cleanup();
        GlobalStyles.Cleanup();
        if (BackgroundMaterial != null) Destroy(BackgroundMaterial);
        if (OutlineMaterial != null) Destroy(OutlineMaterial);
        if (ButtonMaterial != null) Destroy(ButtonMaterial);
        Instance = null;
    }
    private static void Notify(string message) => NotifiLib.SendNotificationALL(message, 3500);
    private static void RefreshLater() { NeedsRefresh = true; }
    private static void RefreshColors() { RefreshLabels(); }
    private static void RefreshLabels()
    {
        foreach (var entry in OptionLabels)
            if (entry.Text != null && entry.Text.text != entry.Button.DisplayName)
            {
                entry.Text.text = entry.Button.DisplayName;
                FitButtonText(entry.Text, entry.Text.text, true);
            }
    }

    public static void OpenPC()
    {
        if (IsMenuOpen) RemoveMenu();
        OnPC = true; ManageEnableMenu();
    }
    public static void ManageEnableMenu()
    {
        if (IsMenuOpen) { RemoveMenu(); return; }
        IsMenuOpen = true;
        CreateMenu();
    }
    public static void CreateMenu()
    {
        if (MenuMain != null) return;
        if (GorillaTagger.Instance == null && !OnPC) { IsMenuOpen = false; return; }
        if (OnPC) SetupDesktopCamera();
        if (BackgroundMaterial == null) CreateMaterials();
        MenuMain = new GameObject("Astra Menu");
        Transform parent = OnPC && PCamera != null ? PCamera.transform : GorillaTagger.Instance!.leftHandTransform;
        MenuMain.transform.SetParent(parent, false);
        MenuMain.transform.localPosition = OnPC ? new Vector3(-0.12f, -0.02f, 0.80f) : VRMenuPosition;
        MenuMain.transform.localRotation = OnPC ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(VRMenuRotation);
        MenuMain.transform.localScale = OnPC ? MenuScale : VRMenuScale;
        if (!OnPC) AlignToPalm();
        // Background
        MenuBackground = CreateCube("Background", MenuMain.transform, new Vector3(0f, -0.14f, 0f), new Vector3(0.15f, 2.1f, 1f), BackgroundMaterial);
        CreateOutline(MenuMain.transform, new Vector3(0.065f, -0.14f, 0f), new Vector3(0.15f, 2.1f, 1f), 0f);
        MenuCanvas = new GameObject("Text Root");
        MenuCanvas.transform.SetParent(MenuMain.transform, false);
        CreateText($"{Constantss.MenuName}", 0.65f, 0f, 0.014f);
        FPS = CreateText("FPS: --", 0.55f, 0f, 0.012f);
        VersionObject = CreateText("v" + Constantss.Version, -1.075f, 0.34f, 0.009f, true).gameObject;
        VersionObject.SetActive(!OnPC);
        CreateCube("Top Divider", MenuMain.transform, new Vector3(-0.10f, 0.43f, 0f), new Vector3(0.03f, 0.009f, 0.9f), OutlineMaterial);
        CreateCube("Bottom Divider", MenuMain.transform, new Vector3(-0.10f, -1.08f, 0f), new Vector3(0.03f, 0.009f, 0.9f), OutlineMaterial);
        CreateReferences();
        RefreshPage();
    }
    private static void AlignToPalm()
    {
        if (MenuMain == null || GorillaTagger.Instance == null) return;
        Transform hand = GorillaTagger.Instance.leftHandTransform;
        Transform head = GorillaTagger.Instance.headCollider.transform;
        if (head == null) return;


        Vector3 toHead = head.position - MenuMain.transform.position;
        toHead.y = 0f;
        if (toHead.sqrMagnitude < 0.0001f) return;

        Vector3 forward = Vector3.Cross(toHead.normalized, Vector3.up);
        Quaternion facing = Quaternion.LookRotation(forward, Vector3.up);
        MenuMain.transform.localRotation = Quaternion.Inverse(hand.rotation) * facing;
    }

    private static void SetupDesktopCamera()
    {
        if (PCamera != null) return;
        GameObject obj = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
        PCamera = obj != null ? obj.GetComponent<Camera>() : null;
        if (PCamera == null) PCamera = Camera.main;
        if (PCamera == null) Debug.LogWarning($"[{Constantss.GUID}] Desktop camera not found.");
    }

    private static void ResetDesktopCamera()
    {
        PCamera = null;
    }

    private static void CreateMaterials()
    {
        Shader shader = Shader.Find("GorillaTag/UberShader") ?? Shader.Find("Standard");
        BackgroundMaterial = new Material(shader) { color = MenuColor };
        OutlineMaterial = new Material(shader) { color = MenuOutlineColor };
        ButtonMaterial = new Material(shader) { color = ButtonColor };
    }
    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 size, Material? material, bool touchable = false)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.layer = 2;
        obj.transform.SetParent(parent, false);

        if (!OnPC && parent == MenuMain?.transform) position.x = VRMenuFaceOffset - position.x;
        obj.transform.localPosition = position;
        obj.transform.localScale = size;
        if (material != null) obj.GetComponent<Renderer>().sharedMaterial = material;
        BoxCollider col = obj.GetComponent<BoxCollider>();
        if (touchable) col.isTrigger = true;
        else Destroy(col);
        return obj;
    }
    private static void CreateOutline(Transform Parent, Vector3 LocalPos, Vector3 Size, float Corner)
    {

        float thick = Mathf.Clamp(Outlinethick, 0.01f, 0.05f);
        CreateCube("Menu Outline", Parent, LocalPos,
            new Vector3(Size.x, Size.y + thick, Size.z + thick), OutlineMaterial);
    }
    private static TextMesh CreateText(string value, float y, float z, float size, bool back = false)
    {
        GameObject obj = new GameObject("Text - " + value);
        obj.transform.SetParent(MenuCanvas!.transform, false);
        float x = back ? 0.16f : -0.19f;
        if (!OnPC) x = VRMenuFaceOffset - x;
        obj.transform.localPosition = new Vector3(x, y, z);
        obj.transform.localRotation = Quaternion.Euler(0f, !OnPC ? (back ? 90f : -90f) : (back ? -90f : 90f), 0f);
        TextMesh text = obj.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.characterSize = size;
        text.fontSize = 48;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        return text;
    }
    public static void RefreshPage()
    {
        if (MenuMain == null) return;
        foreach (GameObject obj in PageObjects) if (obj != null) { obj.SetActive(false); Destroy(obj); }
        PageObjects.Clear(); PageButtons.Clear(); OptionLabels.Clear();
        PageNumber = CategoryManager.PageNumber;
        CreateButton("Disconnect", 1.02f, special: 4);
        string title = CategoryManager.IsHome ? "Main" : CategoryManager.CurrentCategory;
        PageObjects.Add(CreateText(title, 0.34f, 0f, 0.014f).gameObject);
        if (CategoryManager.IsHome)
        {
            var cats = CategoryManager.GetVisibleCategories();
            for (int i = 0; i < cats.Count; i++) CreateButton(cats[i], 0.12f - i * 0.17f, category: cats[i]);
        }
        else
        {
            var buttons = CategoryManager.GetVisibleButtons();
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                float y = 0.12f - i * 0.17f;
                if (button.Type == ButtonType.Option)
                {
                    CreateButton(button.DisplayName, y, button, labelOnly: true);
                    CreateButton("-", y, button, special: 5);
                    CreateButton("+", y, button, special: 6);
                }
                else CreateButton(button.DisplayName, y, button);
            }
        }
        if (!CategoryManager.IsHome) CreateButton("Back", -1.00f, special: 1);
        if (CategoryManager.TotalPages > 1)
        {
            CreateButton("<", -0.82f, special: 2);
            CreateButton(">", -0.82f, special: 3);
        }
        PageObjects.Add(CreateText($"{PageNumber + 1}/{CategoryManager.TotalPages}", -0.82f, 0f, 0.012f).gameObject);
    }
    private static void CreateButton(string label, float y, ButtonManager.ButtonData? button = null, string category = "", int special = 0, bool labelOnly = false)
    {
        float z = special == 3 || special == 5 ? -0.36f : special == 2 || special == 6 ? 0.36f : 0f;
        float width = special >= 2 && special != 4 ? 0.19f : special == 1 ? 0.32f : special == 4 ? 0.95f : 0.89f;
        GameObject obj = CreateCube(label, MenuMain!.transform, new Vector3(-0.12f, y, z), new Vector3(0.07f, special == 4 ? 0.155f : 0.145f, width), ButtonMaterial, !labelOnly);
        PageObjects.Add(obj);
        if (!labelOnly)
        {
            MenuButtonCollider collision = obj.AddComponent<MenuButtonCollider>();
            collision.RelatedButton = button;
            collision.RelatedCategory = category;
            collision.Back = special == 1;
            collision.PreviousPage = special == 2;
            collision.NextPage = special == 3;
            collision.Disconnect = special == 4;
            collision.Incremental = special == 5 || special == 6;
            collision.Positive = special == 6;
            collision.IgnoreToggleColor = collision.Incremental;
            PageButtons.Add(collision);
        }
        TextMesh text = CreateText(label, y, z, 0.014f);
        FitButtonText(text, label, labelOnly, special);
        if (!labelOnly && PageButtons.Count > 0) PageButtons[PageButtons.Count - 1].Label = text;
        if (labelOnly && button != null) OptionLabels.Add((button, text));
        PageObjects.Add(text.gameObject);
    }

    public static void FitButtonText(TextMesh text, string label, bool option = false, int special = 0)
    {
        if (text == null) return;
        float size = special == 5 || special == 6 ? 0.012f : 0.015f;
        int characters = Math.Max(1, label.Length);

        float limit = option ? 0.0105f : 0.0145f;
        if (special == 1 || special == 2 || special == 3) limit = 0.013f;
        size = Mathf.Min(size, limit * (option ? 17f : 21f) / characters);
        text.characterSize = Mathf.Clamp(size, option ? 0.008f : 0.009f, 0.015f);
    }

    public static bool TryGetReferenceHand(Collider collider, out bool rightHand)
    {
        rightHand = collider != null && collider == RightButtonCollider;
        return rightHand;
    }
    public static void CreateReferences()
    {
        if (RightReference != null || GorillaTagger.Instance == null || OnPC) return;
        RightReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        RightReference.name = "Astra Button Pointer";
        RightReference.layer = 2;
        RightReference.transform.SetParent(GorillaTagger.Instance.rightHandTransform, false);
        RightReference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        RightReference.transform.localRotation = Quaternion.identity;
        RightReference.transform.localScale = Vector3.one * 0.012f;
        RightButtonCollider = RightReference.GetComponent<SphereCollider>();
        RightButtonCollider.isTrigger = true;
        Rigidbody rigidbody = RightReference.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        Renderer renderer = RightReference.GetComponent<Renderer>();
        if (renderer != null && OutlineMaterial != null) renderer.sharedMaterial = OutlineMaterial;
    }
    public static void ScheduleAction(Action action)
    {
        if (Instance != null && !Pending) Instance.StartCoroutine(Delay(action));
    }
    private static IEnumerator Delay(Action action)
    {
        Pending = true;
        yield return new WaitForSecondsRealtime(0.5f);
        if (IsMenuOpen) action();
        Pending = false;
    }
    public static void RemoveMenu()
    {
        IsMenuOpen = false; Pending = false;
        if (MenuMain != null) { MenuMain.SetActive(false); Destroy(MenuMain); }
        if (RightReference != null) Destroy(RightReference);
        MenuMain = null; MenuCanvas = null; MenuBackground = null; VersionObject = null; RightReference = null; RightButtonCollider = null; FPS = null;
        PageObjects.Clear(); PageButtons.Clear(); OptionLabels.Clear();
        ResetDesktopCamera();
    }
}

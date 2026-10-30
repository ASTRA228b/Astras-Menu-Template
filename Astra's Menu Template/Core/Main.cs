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
using Astras_Menu_Template.Mods.Settings;
using Astras_Menu_Template.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Astras_Menu_Template.Core; // Astras_Menu_Template

public partial class Main : MonoBehaviour
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
   // private static Transform? PreviousCameraParent;
   // private static Vector3 PreviousCameraPosition;
   // private static Quaternion PreviousCameraRotation;
    private static bool Pending;

    private void Awake()
    {
        Instance = this;
        FileManager.Initialize(this);
        ButtonManager.Initialize();
        MenuThemes.Initialize();
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
        Astras_Menu_Template.Mods.Test.Test.GUIWindow();
    }
    private void Update()
    {
        ButtonManager.Update();
        SoundboardManager.Tick();
        if (!SearchManager.Typing) GUIManager.Tick();
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
        if (keyboard?.hKey.wasPressedThisFrame == true && Application.isFocused && !SearchManager.Typing)
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
                if (MenuMain.transform.parent != PCamera.transform) MenuMain.transform.SetParent(PCamera.transform, false);
                RefreshDesktopBounds();
                if (Mouse.current?.leftButton.wasPressedThisFrame == true)
                {
                    Ray ray = PCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit, 2f, 1 << 2, QueryTriggerInteraction.Collide) && hit.collider.TryGetComponent(out MenuButtonCollider button)) button.Press();
                }
            }
        }
        UpdateSearchInput(keyboard);
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
    private void LateUpdate() { ButtonManager.LateUpdate(); FitVisibleLabels(); }
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
        ReleaseAppearance();
        Instance = null;
    }
    private static void Notify(string message) => NotifiLib.SendNotificationALL(message, 3500);
    private static void RefreshLater() { NeedsRefresh = true; }
    private static void RefreshColors()
    {
        RefreshLabels();
        foreach (var button in PageButtons) if (button != null) button.RefreshAppearance();
    }
    private static void RefreshLabels()
    {
        foreach (var entry in OptionLabels)
            if (entry.Text != null) UpdateButtonLabel(entry.Text, entry.Button.DisplayName);
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
        if (OnPC) { SetupDesktopCamera(); if (PCamera == null) { IsMenuOpen = false; return; } }
        if (BackgroundMaterial == null) CreateMaterials();
        MenuMain = new GameObject("Astra Menu");
        Transform parent = OnPC && PCamera != null ? PCamera.transform : GorillaTagger.Instance!.leftHandTransform;
        MenuMain.transform.SetParent(parent, false);
        MenuMain.transform.localPosition = OnPC ? new Vector3(-0.12f, -0.02f, 0.80f) : VRMenuPosition;
        MenuMain.transform.localRotation = OnPC ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(VRMenuRotation);
        MenuMain.transform.localScale = OnPC ? MenuScale : VRMenuScale;
        if (!OnPC) AlignToPalm();
        MenuCanvas = new GameObject("Text Root");
        MenuCanvas.transform.SetParent(MenuMain.transform, false);
        BuildShell();
        CreateReferences();
        RefreshPage();
        Instance?.BindSearchKeyboard(Keyboard.current);
        RefreshDesktopBounds(true);
        NeedsRefresh = false;
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
        Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("GorillaTag/UberShader") ?? Shader.Find("Standard");
        BackgroundMaterial = new Material(shader) { color = MenuColor };
        OutlineMaterial = new Material(shader) { color = MenuOutlineColor };
        ButtonMaterial = new Material(shader) { color = ButtonColor };
        CreateAppearanceMaterials(shader);
    }
    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 size, Material? material, bool touchable = false)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.layer = 2;
        obj.transform.SetParent(parent, false);

        if (!OnPC && parent == MenuMain?.transform) position.x = VRMenuFaceOffset - position.x;
        if (!OnPC && parent == MenuMain?.transform) position.z = -position.z;
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
        obj.transform.localPosition = new Vector3(x, y, OnPC ? z : -z);
        obj.transform.localRotation = Quaternion.Euler(0f, !OnPC ? (back ? 90f : -90f) : (back ? -90f : 90f), 0f);
        TextMesh text = obj.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.characterSize = size;
        text.fontSize = 48;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = TextColor;
        RegisterText(text, 0.96f, size);
        return text;
    }
    public static void RefreshPage()
    {
        if (MenuMain == null) return;
        foreach (GameObject obj in PageObjects) if (obj != null) { if (obj.TryGetComponent(out TextMesh text)) TextLayouts.Remove(text); obj.SetActive(false); Destroy(obj); }
        PageObjects.Clear(); PageButtons.Clear(); OptionLabels.Clear();
        PageNumber = CategoryManager.PageNumber;
        CreateButton("Disconnect", 1.07f, special: 4);
        CreateButton(SearchManager.Query.Length == 0 ? "Search..." : SearchManager.Query, 0.38f, special: 7);
        string title = SearchManager.Active ? $"SEARCH / {SearchManager.Results.Count} RESULTS" : CategoryManager.IsHome ? "HOME" : CategoryManager.CurrentCategory.ToUpperInvariant();
        TextMesh breadcrumb = CreateText(title, 0.205f, 0f, 0.011f);
        SetTextTone(breadcrumb, 1);
        PageObjects.Add(breadcrumb.gameObject);
        if (CategoryManager.IsHome)
        {
            var cats = CategoryManager.GetVisibleCategories();
            for (int i = 0; i < cats.Count; i++)
            {
                var slot = MenuLayout.GetSlot(i, MenuThemes.Layout);
                CreateButton(cats[i], slot.Y, category: cats[i], centerZ: slot.Z, cellWidth: slot.Width, height: slot.Height);
            }
        }
        else
        {
            var buttons = CategoryManager.GetVisibleButtons();
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                var slot = MenuLayout.GetSlot(i, MenuThemes.Layout);
                if (button.Type == ButtonType.Option)
                {
                    CreateButton(button.DisplayName, slot.Y, button, labelOnly: true, centerZ: slot.Z, cellWidth: slot.Width, height: slot.Height);
                    CreateButton("-", slot.Y, button, special: 5, centerZ: slot.Z, cellWidth: slot.Width, height: slot.Height);
                    CreateButton("+", slot.Y, button, special: 6, centerZ: slot.Z, cellWidth: slot.Width, height: slot.Height);
                }
                else CreateButton(button.DisplayName, slot.Y, button, centerZ: slot.Z, cellWidth: slot.Width, height: slot.Height);
            }
            if (SearchManager.Active && buttons.Count == 0)
            {
                TextMesh empty = CreateText(SearchManager.Query.Trim().Length == 0 ? "Type to find a button" : "No matching buttons", -0.15f, 0f, 0.013f);
                SetTextTone(empty, 1);
                PageObjects.Add(empty.gameObject);
            }
        }
        CreateButton("Home", -0.995f, special: 1);
        if (CategoryManager.TotalPages > 1)
        {
            CreateButton("<", -0.995f, special: 2);
            CreateButton(">", -0.995f, special: 3);
        }
        TextMesh page = CreateText($"PAGE {PageNumber + 1} / {CategoryManager.TotalPages}", -0.83f, 0f, 0.010f);
        SetTextTone(page, 1);
        PageObjects.Add(page.gameObject);
        RefreshSearchKeyboard();
    }
    private static void CreateButton(string label, float y, ButtonManager.ButtonData? button = null, string category = "", int special = 0, bool labelOnly = false, float centerZ = 0f, float cellWidth = 0.97f, float height = 0.15f, string key = "", bool keyboard = false)
    {
        float arrow = cellWidth < 0.7f ? 0.075f : 0.16f;
        float z = special == 2 ? 0.40f : special == 3 ? -0.40f : special == 5 ? centerZ + cellWidth / 2f - arrow / 2f : special == 6 ? centerZ - cellWidth / 2f + arrow / 2f : centerZ;
        float width = special == 2 || special == 3 ? 0.16f : special == 5 || special == 6 ? arrow : special == 1 ? 0.50f : special == 4 || special == 7 ? 0.97f : labelOnly ? cellWidth - 2f * (arrow + 0.02f) : cellWidth;
        GameObject obj = CreateCube(label, MenuMain!.transform, new Vector3(-0.12f, y, z), new Vector3(0.07f, height, width), special == 4 || special == 7 ? HeaderMaterial : ButtonMaterial, !labelOnly);
        StyleButton(obj);
        var objects = keyboard ? KeyboardObjects : PageObjects;
        objects.Add(obj);
        MenuButtonCollider? collision = null;
        if (!labelOnly)
        {
            collision = obj.AddComponent<MenuButtonCollider>();
            collision.RelatedButton = button;
            collision.RelatedCategory = category;
            collision.Back = special == 1;
            collision.PreviousPage = special == 2;
            collision.NextPage = special == 3;
            collision.Disconnect = special == 4;
            collision.Incremental = special == 5 || special == 6;
            collision.Positive = special == 6;
            collision.IgnoreToggleColor = collision.Incremental;
            collision.SearchToggle = special == 7;
            collision.KeyboardKey = key;
            collision.IdleColor = special == 4 || special == 7 ? HeaderColor : ButtonColor;
            if (!keyboard) PageButtons.Add(collision);
        }
        float textWidth = Mathf.Max(0.035f, width - (width < 0.2f ? 0.025f : 0.05f));
        TextMesh text = CreateText(FormatButtonLabel(label, textWidth), y, z, 0.014f);
        RegisterText(text, textWidth, height < 0.13f ? 0.012f : 0.015f, height - 0.025f);
        if (collision != null) collision.Label = text;
        if (labelOnly && button != null) OptionLabels.Add((button, text));
        objects.Add(text.gameObject);
        collision?.RefreshAppearance();
    }

    public static void FitButtonText(TextMesh text, string label, bool option = false, int special = 0)
    {
        if (text == null) return;
        float width = option ? 0.56f : special == 2 || special == 3 || special == 5 || special == 6 ? 0.12f : special == 1 ? 0.44f : 0.89f;
        RegisterText(text, width, special == 2 || special == 3 ? 0.018f : 0.015f);
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
        yield return new WaitForSecondsRealtime(0.12f);
        if (IsMenuOpen) action();
        Pending = false;
    }
    public static void RemoveMenu()
    {
        IsMenuOpen = false; Pending = false;
        Instance?.BindSearchKeyboard(null);
        bool wasSearching = SearchManager.Active;
        SearchManager.Close();
        if (wasSearching) CategoryManager.SearchChanged();
        ClearSearchKeyboard();
        if (MenuMain != null) { MenuMain.SetActive(false); Destroy(MenuMain); }
        if (RightReference != null) Destroy(RightReference);
        MenuMain = null; MenuCanvas = null; MenuBackground = null; RightReference = null; RightButtonCollider = null; FPS = null;
        PageObjects.Clear(); PageButtons.Clear(); OptionLabels.Clear();
        TextLayouts.Clear();
        ResetDesktopCamera();
    }
}

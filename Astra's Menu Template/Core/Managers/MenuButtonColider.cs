using UnityEngine;
using Photon.Pun;
using Astras_Menu_Template.Stuff;
using Astras_Menu_Template.Mods.Settings;
using static Astras_Menu_Template.Core.Main;

namespace Astras_Menu_Template.Core.Managers;

public class MenuButtonCollider : MonoBehaviour
{
    public static float ButtonCooldown;
    public ButtonManager.ButtonData? RelatedButton;
    public string RelatedCategory = "";
    public bool Incremental, Positive, Back, NextPage, PreviousPage, Disconnect;
    public bool IgnoreToggleColor;
    public bool SearchToggle;
    public string KeyboardKey = "";
    public Color IdleColor;
    public TextMesh? Label;
    private Renderer? Render;
    private MaterialPropertyBlock? Properties;
    private float PressedUntil;
    private Color lastColor;
    private bool hasColor;

    private void Awake() { Render = GetComponent<Renderer>(); Properties = new MaterialPropertyBlock(); IdleColor = ButtonColor; }
    private void Update() => RefreshAppearance();
    public void RefreshAppearance()
    {
        if (Render == null || Properties == null) return;
        bool active = SearchToggle && SearchManager.Typing || !IgnoreToggleColor && RelatedButton?.Type == ButtonType.Toggle && RelatedButton.Enabled;
        bool highlighted = active || Time.unscaledTime < PressedUntil;
        Color color = highlighted ? BottonPressedColor : IdleColor;
        if (!hasColor || color != lastColor)
        {
            Properties.SetColor("_Color", color);
            Render.SetPropertyBlock(Properties);
            lastColor = color; hasColor = true;
        }
        Render.enabled = SearchToggle || MenuThemes.ButtonStyle == 0 || MenuThemes.ButtonStyle == 1 && highlighted;
        if (Label != null) Label.color = highlighted ? MenuThemes.ButtonStyle == 2 ? MenuOutlineColor : ActiveTextColor : TextColor;
        if (Label != null && SearchToggle) Astras_Menu_Template.Core.Main.UpdateButtonLabel(Label, SearchManager.Query.Length == 0 ? "Search..." : SearchManager.Query);
        if (Label != null && RelatedButton != null && !Incremental) { string value = RelatedButton.Name switch
        {
            "Play / Stop" => SoundboardManager.IsPlaying ? "Stop Sound" : "Play Sound",
            "Next Sound" => "Next: " + SoundboardManager.CurrentName,
            "Autoplay" => $"Autoplay [{(SoundboardManager.AutoPlay ? "ON" : "OFF")}]",
            "Repeat" => $"Repeat [{(SoundboardManager.Repeat ? "ON" : "OFF")}]",
            "Shuffle" => $"Shuffle [{(SoundboardManager.Shuffle ? "ON" : "OFF")}]",
            _ => RelatedButton.DisplayName
        }; Astras_Menu_Template.Core.Main.UpdateButtonLabel(Label, value); }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetReferenceHand(other, out bool rightHand)) return;
        Press(rightHand);
    }
    public void Press(bool? rightHand = null)
    {
        if (MenuMain == null || Time.unscaledTime < ButtonCooldown) return;
        ButtonCooldown = Time.unscaledTime + 0.16f;
        PressedUntil = Time.unscaledTime + 0.18f;
        if (rightHand.HasValue && GorillaTagger.Instance != null)
        {
            GorillaTagger.Instance.StartVibration(rightHand.Value, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
        }
        if (VRRig.LocalRig != null) VRRig.LocalRig.PlayHandTapLocal(8, rightHand ?? true, 0.4f);
        RefreshAppearance();
        if (SearchToggle) { SearchManager.Toggle(); return; }
        if (KeyboardKey.Length > 0) { SearchManager.Key(KeyboardKey); return; }
        if (Disconnect) { ScheduleAction(() => { if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect(); }); return; }
        if (Back) { ScheduleAction(CategoryManager.Back); return; }
        if (NextPage) { ScheduleAction(CategoryManager.NextPage); return; }
        if (PreviousPage) { ScheduleAction(CategoryManager.PreviousPage); return; }
        if (!string.IsNullOrEmpty(RelatedCategory)) { string cat = RelatedCategory; ScheduleAction(() => CategoryManager.Open(cat)); return; }
        if (RelatedButton == null) return;
        if (Incremental) ButtonManager.ChangeOption(RelatedButton, Positive ? 1 : -1);
        else ButtonManager.Press(RelatedButton);
    }
}

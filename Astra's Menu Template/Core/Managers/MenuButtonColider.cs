using UnityEngine;
using Photon.Pun;
using Astras_Menu_Template.Stuff;
using static Astras_Menu_Template.Core.Main;

namespace Astras_Menu_Template.Core.Managers;

public class MenuButtonCollider : MonoBehaviour
{
    public static float ButtonCooldown;
    public ButtonManager.ButtonData? RelatedButton;
    public string RelatedCategory = "";
    public bool Incremental, Positive, Back, NextPage, PreviousPage, Disconnect;
    public bool IgnoreToggleColor;
    public TextMesh? Label;
    private Renderer? Render;
    private MaterialPropertyBlock? Properties;
    private float PressedUntil;

    private void Awake() { Render = GetComponent<Renderer>(); Properties = new MaterialPropertyBlock(); }
    private void Update()
    {
        if (Render == null || Properties == null) return;
        bool active = !IgnoreToggleColor && RelatedButton?.Type == ButtonType.Toggle && RelatedButton.Enabled;
        Properties.SetColor("_Color", active || Time.unscaledTime < PressedUntil ? BottonPressedColor : ButtonColor);
        Render.SetPropertyBlock(Properties);
        if (Label != null && RelatedButton != null && !Incremental) { string value = RelatedButton.Name switch
        {
            "Play / Stop" => SoundboardManager.IsPlaying ? "Stop Sound" : "Play Sound",
            "Next Sound" => "Next: " + SoundboardManager.CurrentName,
            "Autoplay" => $"Autoplay [{(SoundboardManager.AutoPlay ? "ON" : "OFF")}]",
            "Repeat" => $"Repeat [{(SoundboardManager.Repeat ? "ON" : "OFF")}]",
            "Shuffle" => $"Shuffle [{(SoundboardManager.Shuffle ? "ON" : "OFF")}]",
            _ => RelatedButton.DisplayName
        }; if (Label.text != value) { Label.text = value; Astras_Menu_Template.Core.Main.FitButtonText(Label, value); } }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetReferenceHand(other, out bool rightHand)) return;
        Press(rightHand);
    }
    public void Press(bool? rightHand = null)
    {
        if (MenuMain == null || Time.unscaledTime < ButtonCooldown) return;
        ButtonCooldown = Time.unscaledTime + 0.20f;
        PressedUntil = Time.unscaledTime + 0.5f;
        if (rightHand.HasValue && GorillaTagger.Instance != null)
        {
            GorillaTagger.Instance.StartVibration(rightHand.Value, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
            if (VRRig.LocalRig != null) VRRig.LocalRig.PlayHandTapLocal(8, rightHand.Value, 0.4f);
        }
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

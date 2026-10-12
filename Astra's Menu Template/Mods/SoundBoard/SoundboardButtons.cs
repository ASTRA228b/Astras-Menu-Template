using Astras_Menu_Template.Core.Managers;
using Astras_Menu_Template.Stuff;

namespace Astras_Menu_Template.Mods.SoundBoard;

public static class SoundboardButtons
{
    [MenuButton("Play / Stop", ButtonType.Press, Category = "Soundboard", Order = 0)]
    public static void PlayStop() => SoundboardManager.TogglePlay();

    [MenuButton("Next Sound", ButtonType.Press, Category = "Soundboard", Order = 1)]
    public static void NextSound() => SoundboardManager.Next();

    [MenuButton("Volume", ButtonType.Option, Category = "Soundboard", Order = 2, Options = new[] { "25%", "50%", "75%", "100%" })]
    public static void Volume() => SoundboardManager.SetVolume(ButtonManager.GetButton("Volume")!.OptionIndex);

    [MenuButton("Sound Output", ButtonType.Option, Category = "Soundboard", Order = 3, Options = new[] { "Local", "Microphone", "Both" })]
    public static void Output() => SoundboardManager.SetPlaybackMode((SoundManager.PlaybackMode)ButtonManager.GetButton("Sound Output")!.OptionIndex);

    [MenuButton("Mic Volume", ButtonType.Option, Category = "Soundboard", Order = 4, Options = new[] { "25%", "50%", "75%", "100%" })]
    public static void MicVolume() => SoundboardManager.SetMicVolume(ButtonManager.GetButton("Mic Volume")!.OptionIndex);

    [MenuButton("Autoplay", ButtonType.Press, Category = "Soundboard", Order = 5)]
    public static void Autoplay() => SoundboardManager.SetAutoPlay(!SoundboardManager.AutoPlay);

    [MenuButton("Repeat", ButtonType.Press, Category = "Soundboard", Order = 6)]
    public static void Repeat() => SoundboardManager.SetRepeat(!SoundboardManager.Repeat);

    [MenuButton("Shuffle", ButtonType.Press, Category = "Soundboard", Order = 7)]
    public static void Shuffle() => SoundboardManager.SetShuffle(!SoundboardManager.Shuffle);

    [MenuButton("Open Soundboard GUI", ButtonType.Press, Category = "Soundboard", Order = 8)]
    public static void GUI() => SoundboardGUI.Visible = !SoundboardGUI.Visible;

    [MenuButton("Reload Sounds", ButtonType.Press, Category = "Soundboard", Order = 9)]
    public static void Reload() => SoundboardManager.Reload();
}

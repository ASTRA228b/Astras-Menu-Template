using Astras_Menu_Template.Stuff;
using BepInEx;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Astras_Menu_Template.Core.Managers;

public static class FileManager
{
    private const string SoundURL = "https://iimenu-lts-serverdata.vercel.app/nt4.mp3";

    public static string RootFolder => Path.Combine(Paths.GameRootPath, $"{Constantss.GameRootPath}");
    public static string SoundFolder => Path.Combine(RootFolder, "Sounds");
    public static string SoundLinks => Path.Combine(RootFolder, "sounds.txt");
    public static string SettingsPath => Path.Combine(RootFolder, "settings.json");

    private static bool Initialized;
    private static bool Downloading;

    public sealed class SoundEntry
    {
        public string Name = "";
        public string Source = "";

        public SoundEntry(string name, string source)
        {
            Name = name;
            Source = source;
        }
    }

    public static void Initialize(MonoBehaviour R)
    {
        if (Initialized) return;

        try
        {
            Directory.CreateDirectory(SoundFolder);

            string readme = Path.Combine(SoundFolder, "README.txt");
            string sample = Path.Combine(SoundFolder, "nt4.mp3");

            if (!File.Exists(readme))
                File.WriteAllText(readme, $"{Constantss.MenuName} - SOUNDS\n\nPlace MP3, WAV, or OGG audio in this folder.\nIn the parent folder, add direct HTTPS links to sounds.txt, one per line.\nOptional names: My Sound | https://example.com/audio.mp3\nLines starting with # are ignored. Use Reload Sounds in the GUI.\nOnly use audio you have permission to play.\n");

            if (!File.Exists(SoundLinks))
                File.WriteAllText(SoundLinks, "# Direct HTTPS audio URLs, one per line\n# My Sound | https://example.com/audio.mp3\n");

            if (!File.Exists(sample) && R != null)
                R.StartCoroutine(DownloadSounds(sample));

            Initialized = true;

            Debug.Log($"[{Constantss.GUID}] Sound folder initialized.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] FileManager: {ex}");
        }
    }

    public static List<SoundEntry> GetSounds()
    {
        List<SoundEntry> sounds = new();

        try
        {
            if (Directory.Exists(SoundFolder))
            {
                foreach (string file in Directory.GetFiles(SoundFolder))
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();

                    if (ext == ".mp3" || ext == ".wav" || ext == ".ogg")
                        sounds.Add(new SoundEntry(Path.GetFileNameWithoutExtension(file), file));
                }
            }

            if (File.Exists(SoundLinks))
            {
                foreach (string raw in File.ReadAllLines(SoundLinks))
                {
                    string line = raw.Trim();

                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    int split = line.IndexOf('|');
                    string url = (split >= 0 ? line.Substring(split + 1) : line).Trim();

                    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps)
                        continue;

                    string ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();

                    if (ext != ".mp3" && ext != ".wav" && ext != ".ogg")
                        continue;

                    string name = split >= 0 ? line.Substring(0, split).Trim() : Path.GetFileNameWithoutExtension(uri.AbsolutePath);

                    sounds.Add(new SoundEntry(string.IsNullOrWhiteSpace(name) ? "Remote Sound" : name, url));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] Sounds: {ex}");
        }

        return sounds;
    }

    public static IEnumerator DownloadSounds(string SavePath)
    {
        if (File.Exists(SavePath) || Downloading)
            yield break;

        Downloading = true;

        using UnityWebRequest req = UnityWebRequest.Get(SoundURL);
        req.timeout = 12;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[{Constantss.GUID}] Optional sample sound unavailable: {req.error}");
            Downloading = false;
            yield break;
        }

        try
        {
            if (!File.Exists(SavePath))
            {
                File.WriteAllBytes(SavePath, req.downloadHandler.data);
                Debug.Log($"[{Constantss.GUID}] Downloaded nt4.mp3"); // nt4.mp3 is the test sound, you can change it but its a good sound.
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] Download save failed: {ex}");
        }

        Downloading = false;
    }
}

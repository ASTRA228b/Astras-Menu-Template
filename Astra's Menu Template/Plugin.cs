using UnityEngine;
using BepInEx;
using Astras_Menu_Template.Core;
using Astras_Menu_Template.Stuff;
using Astras_Menu_Template.Libraries;

namespace Astras_Menu_Template;

[BepInPlugin(Constantss.GUID, Constantss.Name, Constantss.Version)]
public class Plugin : BaseUnityPlugin
{
    private void Start()
    {
       PatchLoader.Apply(); // Call this method to load everypatch in this menu. 
    }

    private void Awake()
    {
        GameObject Plugin = new GameObject(Constantss.ObjectName);
        Plugin.AddComponent<Main>();
        DontDestroyOnLoad(Plugin);
    }
}
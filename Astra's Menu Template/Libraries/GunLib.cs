using UnityEngine;
using Astras_Menu_Template.Stuff;

namespace Astras_Menu_Template.Libraries;

public static class GunLib // WARNING: This might be very buggy, buggy, or just not working the way it was intended to. Me myself as the develepor making this comment will tell you i will not be making anything telling you how to use it.
{
    public static GameObject? Pointer;
    private static LineRenderer? Line;
    public static RaycastHit LastHit;
    public static bool HasHit;
    public static bool Enabled;

    public static bool UpdateGun(Transform hand, bool active, bool select, out Vector3 point, float distance = 80f)
    {
        point = Vector3.zero;
        if (!active || hand == null) { Hide(); return false; }
        if (Line == null) Create();
        Vector3 origin = hand.position;
        Vector3 direction = hand.forward;
        HasHit = Physics.Raycast(origin, direction, out LastHit, distance, ~0, QueryTriggerInteraction.Ignore);
        point = HasHit ? LastHit.point : origin + direction * distance;
        if (Pointer != null) { Pointer.SetActive(true); Pointer.transform.position = point; }
        if (Line != null) { Line.enabled = true; Line.SetPosition(0, origin); Line.SetPosition(1, point); }
        Enabled = true;
        return select && HasHit;
    }

    private static void Create()
    {
        GameObject root = new GameObject(Constantss.GUID + ".GunLine");
        Line = root.AddComponent<LineRenderer>();
        Line.positionCount = 2;
        Line.startWidth = 0.008f;
        Line.endWidth = 0.004f;
        Line.useWorldSpace = true;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) Line.material = new Material(shader);
        Line.startColor = Color.magenta;
        Line.endColor = Color.white;
        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Pointer.name = Constantss.GUID + ".GunPointer";
        Pointer.transform.localScale = Vector3.one * 0.035f;
        UnityEngine.Object.Destroy(Pointer.GetComponent<Collider>());
    }
    public static void Hide() { Enabled = false; if (Line != null) Line.enabled = false; if (Pointer != null) Pointer.SetActive(false); }
    public static void Cleanup()
    {
        if (Line != null) { if (Line.material != null) UnityEngine.Object.Destroy(Line.material); UnityEngine.Object.Destroy(Line.gameObject); }
        if (Pointer != null) UnityEngine.Object.Destroy(Pointer);
        Line = null; Pointer = null; HasHit = false; Enabled = false;
    }
}

namespace Astras_Menu_Template.Core;

internal static class MenuLayout
{
    internal readonly struct Slot
    {
        internal readonly float Y, Z, Width, Height;
        internal Slot(float y, float z, float width, float height) { Y = y; Z = z; Width = width; Height = height; }
    }
    internal static int PageSize(int layout) => layout switch { 1 => 7, 2 => 3, 3 => 8, _ => 5 };
    internal static Slot GetSlot(int index, int layout) => layout switch
    {
        1 => new(0.055f - index * 0.13f, 0f, 0.97f, 0.11f),
        2 => new(0.025f - index * 0.30f, 0f, 0.97f, 0.25f),
        3 => new(0.035f - index / 2 * 0.235f, index % 2 == 0 ? 0.25f : -0.25f, 0.47f, 0.18f),
        _ => new(0.035f - index * 0.18f, 0f, 0.97f, 0.15f)
    };
}

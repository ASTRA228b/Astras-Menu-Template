using System.Globalization;

namespace Astras_Menu_Template.Core.Managers;

public static class SearchManager
{
    private static readonly List<ButtonManager.ButtonData> Matches = new();
    public static string Query { get; private set; } = "";
    public static bool Active { get; private set; }
    public static bool Typing { get; private set; }
    public static IReadOnlyList<ButtonManager.ButtonData> Results => Matches;

    public static void Toggle()
    {
        if (Typing) { Blur(); return; }
        Active = Typing = true;
        Rebuild();
        CategoryManager.SearchChanged();
    }
    public static void Blur()
    {
        Typing = false;
        if (Query.Length == 0) Active = false;
        CategoryManager.SearchChanged();
    }
    public static void Close() { Active = Typing = false; Query = ""; Matches.Clear(); }
    public static void Key(string value)
    {
        if (!Typing) return;
        if (value == "Done") { Blur(); return; }
        if (value == "Del") { if (Query.Length == 0) return; Query = Query.Substring(0, Query.Length - 1); }
        else if (value == "Clear") Query = "";
        else if (value == "Space") { if (Query.Length >= 32) return; Query += " "; }
        else if (value.Length == 1 && !char.IsControl(value[0]) && !char.IsSurrogate(value[0]) && Query.Length < 32) Query += value.ToLowerInvariant();
        else return;
        Rebuild();
        CategoryManager.SearchChanged();
    }
    private static bool Contains(string? value, string query) => !string.IsNullOrEmpty(value) && CultureInfo.InvariantCulture.CompareInfo.IndexOf(value, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    private static void Rebuild()
    {
        Matches.Clear();
        string query = Query.Trim();
        if (!Active || query.Length == 0) return;
        foreach (var button in ButtonManager.Buttons)
            if (Contains(button.Name, query) || Contains(button.Attribute.Description, query) || Contains(button.Attribute.Controls, query) || Contains(button.Category, query) || button.Attribute.Options?.Any(option => Contains(option, query)) == true) Matches.Add(button);
    }
}

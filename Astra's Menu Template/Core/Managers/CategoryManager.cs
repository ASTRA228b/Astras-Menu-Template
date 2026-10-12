using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astras_Menu_Template.Core.Managers;

public static class CategoryManager
{
    public const string Home = "Main";// This is the hame category name     |       // remove this or add more here if you want to.
    private static readonly List<string> CategoriesInternal = new() { "Settings", "Test" ,"Soundboard" }; // You don't need to hardcode categorys here, whenever you add a button with a category it will autocreate it.
    public static string CurrentCategory { get; private set; } = Home;
    public static int PageNumber { get; private set; }
    public static int PageLimit = 5; // I really have no idea why i added this.
    public static bool IsHome => CurrentCategory == Home;
    public static event Action? OnPageChanged;

    public static void Initialize()
    {
        foreach (string category in ButtonManager.GetCategories())
            if (!string.Equals(category, Home, StringComparison.OrdinalIgnoreCase) && !CategoriesInternal.Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase))) CategoriesInternal.Add(category);
        Back();
    }
    public static IReadOnlyList<string> GetVisibleCategories() => CategoriesInternal.Skip(PageNumber * Mathf.Max(1, PageLimit)).Take(Mathf.Max(1, PageLimit)).ToList();
    public static IReadOnlyList<ButtonManager.ButtonData> GetVisibleButtons() => IsHome ? new List<ButtonManager.ButtonData>() : ButtonManager.GetCategory(CurrentCategory).Skip(PageNumber * Mathf.Max(1, PageLimit)).Take(Mathf.Max(1, PageLimit)).ToList();
    public static int TotalPages
    {
        get
        {
            int count = IsHome ? CategoriesInternal.Count : ButtonManager.GetCategory(CurrentCategory).Count;
            return Mathf.Max(1, Mathf.CeilToInt(count / (float)Mathf.Max(1, PageLimit)));
        }
    }
    public static void Open(string category)
    {
        string? found = CategoriesInternal.FirstOrDefault(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
        if (found == null) return;
        CurrentCategory = found;
        PageNumber = 0;
        OnPageChanged?.Invoke();
    }
    public static void Back() { CurrentCategory = Home; PageNumber = 0; OnPageChanged?.Invoke(); }
    public static void NextPage() { PageNumber = (PageNumber + 1) % TotalPages; OnPageChanged?.Invoke(); }
    public static void PreviousPage() { PageNumber = (PageNumber + TotalPages - 1) % TotalPages; OnPageChanged?.Invoke(); }
}

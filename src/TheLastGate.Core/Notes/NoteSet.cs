using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLastGate.Notes;

// The player's map notes: categories nested up to eight deep, kept in the order the tree shows them among siblings.
// Categories and notes share one id counter, starting at 1; 0 means none.
public sealed class NoteSet
{
    private readonly List<NoteCategory> categories = [];
    private int nextId = 1;

    public IReadOnlyList<NoteCategory> Categories => categories;

    public bool Changed { get; set; }

    public int AddCategory(string name, int colourIndex, int parentId = 0)
    {
        if (!AcceptsChild(parentId) || categories.Count >= NoteLimits.Categories)
        {
            return 0;
        }

        var category = new NoteCategory
        {
            Id = nextId++,
            ParentId = parentId,
            Name = UniqueName(parentId, 0, name),
            ColourIndex = IsPaletteColour(colourIndex) ? colourIndex : 0,
        };
        categories.Add(category);
        Changed = true;
        return category.Id;
    }

    public NoteCategory? FindCategory(int categoryId) =>
        categories.FirstOrDefault(category => category.Id == categoryId);

    public bool HasChildren(int categoryId) =>
        categoryId != 0 && categories.Any(category => category.ParentId == categoryId && category.Id != categoryId);

    public int Depth(int categoryId)
    {
        var depth = 0;
        for (var current = FindCategory(categoryId);
            current != null && depth <= NoteLimits.Depth;
            current = FindCategory(current.ParentId))
        {
            depth++;
        }

        return depth;
    }

    public int SubtreeHeight(int categoryId) => FindCategory(categoryId) == null ? 0 : HeightFrom(categoryId, 1);

    public bool IsWithin(int categoryId, int ancestorId)
    {
        var current = categoryId;
        for (var step = 0; step <= NoteLimits.Depth && current != 0; step++)
        {
            if (current == ancestorId)
            {
                return true;
            }

            current = FindCategory(current)?.ParentId ?? 0;
        }

        return false;
    }

    public bool IsShown(NoteCategory category) => AllTheWayUp(category, shown => shown.Visible);

    public bool LabelsShown(NoteCategory category) => AllTheWayUp(category, shown => shown.ShowLabels);

    public IReadOnlyList<NoteCategory> Tree(int rootId = 0)
    {
        var ordered = new List<NoteCategory>();
        if (rootId != 0)
        {
            if (FindCategory(rootId) is { } root)
            {
                Collect(root, 1, ordered);
            }

            return ordered;
        }

        foreach (var topLevel in categories.Where(category => category.ParentId == 0).ToList())
        {
            Collect(topLevel, 1, ordered);
        }

        return ordered;
    }

    public bool RemoveCategory(int categoryId)
    {
        if (FindCategory(categoryId) is not { } category || HasChildren(categoryId))
        {
            return false;
        }

        categories.Remove(category);
        Changed = true;
        return true;
    }

    public bool RenameCategory(int categoryId, string name)
    {
        if (FindCategory(categoryId) is not { } category || name.Length == 0)
        {
            return false;
        }

        category.Name = UniqueName(category.ParentId, categoryId, name);
        Changed = true;
        return true;
    }

    public bool SetCategoryColour(int categoryId, int colourIndex)
    {
        if (FindCategory(categoryId) is not { } category || !IsPaletteColour(colourIndex))
        {
            return false;
        }

        category.ColourIndex = colourIndex;
        Changed = true;
        return true;
    }

    // A category can't move under itself, and its whole subtree must still fit within eight levels.
    public bool CanMoveCategory(int categoryId, int newParentId) =>
        FindCategory(categoryId) is { } category
        && newParentId != category.ParentId
        && (newParentId == 0 || FindCategory(newParentId) != null)
        && !IsWithin(newParentId, categoryId)
        && Depth(newParentId) + SubtreeHeight(categoryId) <= NoteLimits.Depth;

    public bool MoveCategory(int categoryId, int newParentId)
    {
        if (!CanMoveCategory(categoryId, newParentId))
        {
            return false;
        }

        var category = FindCategory(categoryId)!;
        category.Name = UniqueName(newParentId, categoryId, category.Name);
        category.ParentId = newParentId;
        Changed = true;
        return true;
    }

    public bool CanShiftCategory(int categoryId, int direction) => NeighbourIndex(categoryId, direction) >= 0;

    public bool ShiftCategory(int categoryId, int direction)
    {
        var neighbour = NeighbourIndex(categoryId, direction);
        if (neighbour < 0)
        {
            return false;
        }

        var index = categories.FindIndex(category => category.Id == categoryId);
        (categories[index], categories[neighbour]) = (categories[neighbour], categories[index]);
        Changed = true;
        return true;
    }

    private static bool IsPaletteColour(int colourIndex) => colourIndex is >= 0 and < NotePalette.Size;

    private bool AcceptsChild(int parentId) =>
        parentId == 0 || (FindCategory(parentId) != null && Depth(parentId) < NoteLimits.Depth);

    private int HeightFrom(int categoryId, int level)
    {
        if (level > NoteLimits.Depth)
        {
            return 0;
        }

        var tallest = categories
            .Where(category => category.ParentId == categoryId && category.Id != categoryId)
            .Select(child => HeightFrom(child.Id, level + 1))
            .DefaultIfEmpty(0)
            .Max();
        return tallest + 1;
    }

    private bool AllTheWayUp(NoteCategory category, Func<NoteCategory, bool> isOn)
    {
        var current = category;
        for (var step = 0; step <= NoteLimits.Depth && current != null; step++)
        {
            if (!isOn(current))
            {
                return false;
            }

            current = FindCategory(current.ParentId);
        }

        return true;
    }

    private void Collect(NoteCategory category, int level, List<NoteCategory> ordered)
    {
        ordered.Add(category);
        if (level >= NoteLimits.Depth)
        {
            return;
        }

        foreach (var child in categories.Where(child => child != category && child.ParentId == category.Id).ToList())
        {
            Collect(child, level + 1, ordered);
        }
    }

    // A name taken by a sibling, ignoring ASCII case, gets " (2)", " (3)" and so on, cut to keep within the limit.
    private string UniqueName(int parentId, int ignoredId, string wanted)
    {
        var name = NoteText.Fit(wanted, NoteLimits.CategoryNameBytes);
        for (var attempt = 2; NameInUse(parentId, ignoredId, name); attempt++)
        {
            var suffix = $" ({attempt})";
            name = NoteText.Fit(wanted, NoteLimits.CategoryNameBytes - suffix.Length) + suffix;
        }

        return name;
    }

    private bool NameInUse(int parentId, int ignoredId, string name) =>
        categories.Any(category =>
            category.Id != ignoredId && category.ParentId == parentId && NoteText.SameName(category.Name, name));

    private int NeighbourIndex(int categoryId, int direction)
    {
        var index = categories.FindIndex(category => category.Id == categoryId);
        if (index < 0 || direction is not (-1 or 1))
        {
            return -1;
        }

        for (var candidate = index + direction;
            candidate >= 0 && candidate < categories.Count;
            candidate += direction)
        {
            if (categories[candidate].ParentId == categories[index].ParentId)
            {
                return candidate;
            }
        }

        return -1;
    }
}

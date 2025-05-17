using System.Text.Json;

namespace WindowOrganizer;

public class GridConfig
{
    public int DefaultColumns { get; set; } = 4;
    public int DefaultRows { get; set; } = 2;
    public List<ScreenConfig>? Screens { get; set; }

    public static GridConfig Load(string path = "config.json")
    {
        if (!File.Exists(path))
            return new GridConfig();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GridConfig>(json) ?? new GridConfig();
    }

    public (int Columns, int Rows) GetGridSize(int screenIndex)
    {
        if (Screens != null && screenIndex < Screens.Count)
            return (Screens[screenIndex].Columns, Screens[screenIndex].Rows);
        return (DefaultColumns, DefaultRows);
    }
}

public class ScreenConfig
{
    public int Columns { get; set; }
    public int Rows { get; set; }
} 
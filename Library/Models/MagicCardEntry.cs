namespace Library.Models;

public class MagicCardEntry
{
    public int Quantity { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> ImageUrls { get; set; } = new();
}

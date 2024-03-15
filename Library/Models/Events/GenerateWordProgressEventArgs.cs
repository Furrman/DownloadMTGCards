namespace Library.Models.Events;

public class GenerateWordProgressEventArgs : EventArgs
{
    public double? Percent { get; set; }
    public string? ErrorMessage { get; set; }
}

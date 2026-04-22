namespace NavData.Services.Models;

public class CombinedSequence
{
    public string Transition { get; set; } = string.Empty;
    public string TransitionType { get; set; } = string.Empty;
    public List<Point> Points { get; set; } = [];
}

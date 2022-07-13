namespace ShiftScreen.Settings;

public class CaptureSettings
{
    public Point Start { get; set; } = new();
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public int Fps { get; set; } = 30;
}

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}
namespace RaylibGameFramework.Configuration;

public sealed class GameConfig
{
    public int WindowWidth { get; set; } = 1280;

    public int WindowHeight { get; set; } = 720;

    public string WindowTitle { get; set; } = "Framework Test";

    public int TargetFps { get; set; } = 60;

    public bool VSync { get; set; } = true;

    public bool Fullscreen { get; set; }
}

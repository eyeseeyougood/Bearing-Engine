namespace Bearing;

public interface IRenderable
{
    public int rid { get; set; }

    public bool isTransparent { get; set; }
    public int renderPass { get; set; }

    public void Render() { }
}
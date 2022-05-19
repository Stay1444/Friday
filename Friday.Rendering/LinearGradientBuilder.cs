
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Friday.Rendering;

public class LinearGradientBuilder
{
    public PointF Start { get; }
    public PointF End { get; }
    private List<Rgba32> _colors = new List<Rgba32>();
    public LinearGradientBuilder(PointF start, PointF end)
    {
        Start = start;
        End = end;
    }

    public void AddColor(Rgba32 c)
    {
        _colors.Add(c);
    }
    
    public LinearGradientBrush Build()
    {
        var stops = new List<ColorStop>();
        var stepSize = 1f / (_colors.Count - 1);
        for (var i = 0; i < _colors.Count; i++)
        {
            var c = _colors[i];
            var offset = i * stepSize;
            stops.Add(new ColorStop(offset, c));
        }
        
        return new LinearGradientBrush(Start, End,GradientRepetitionMode.Repeat, stops.ToArray());
    }
}
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Friday.Rendering;

public static class Extensions
{
    public static void CropCircle(this IImageProcessingContext context, int width, int height)
    {
        // first we crop the image to a rectangle(width, height)
        context.Crop(width, height);

        var radius = Math.Min(width, height) / 2;

        // create the paths for the circle
        var rect = new RectangularPolygon(-0.5f, -0.5f, radius, radius);
        var cornerTopLeft = rect.Clip(new EllipsePolygon(radius - 0.5f, radius - 0.5f, radius));
        
        var rightPos = width - cornerTopLeft.Bounds.Width + 1;
        var bottomPos = height - cornerTopLeft.Bounds.Height + 1;
        
        
        var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
        var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
        var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

        var pathCollection = new PathCollection(cornerTopLeft, cornerTopRight, cornerBottomRight, cornerBottomLeft);

        context.SetGraphicsOptions(new GraphicsOptions()
        {
            Antialias = true,
            AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
        });

        foreach (var corner in pathCollection)
        {
            context = context.Fill(Color.Red, corner);
        }
    }
    
    public static void DrawProgressBar(this IImageProcessingContext ctx, int width, int height, PointF location, float progress, Rgba32 color, Rgba32 background)
    {
        int ProgressToWidth(float p)
        {
            return (int)(width * progress);
        }

        location.X = location.X - 10;
        ctx.FillPolygon(background, new EllipsePolygon(new PointF(location.X + (int)(height / 2), location.Y + (int)(height / 2)), (int)(height / 2)).Points.ToArray());
        ctx.Fill(background, new RectangleF(location.X + (int)(height / 2), location.Y, width - (int)(height / 2), height));
        ctx.FillPolygon(background, new EllipsePolygon(new PointF(location.X + width, location.Y + (int)(height / 2)), (int)(height / 2)).Points.ToArray());

        if (progress > 0)
        {
            ctx.FillPolygon(color, new EllipsePolygon(new PointF(location.X + (int)(height / 2), location.Y + (int)(height / 2)), (int)(height / 2)).Points.ToArray());
            ctx.Fill(color, new RectangleF(location.X + (int)(height / 2), location.Y, ProgressToWidth(progress), height));
            ctx.FillPolygon(color, new EllipsePolygon(new PointF(location.X + ProgressToWidth(progress) + (int)(height / 2), location.Y + (int)(height / 2)), (int)(height / 2)).Points.ToArray());

        }
    }
}
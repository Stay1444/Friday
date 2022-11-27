using DSharpPlus.Lavalink;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Friday.Modules.Music;

public static class Extensions
{
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

    public static string GetVideoId(this LavalinkTrack video)
    {
        var url = video.Uri;
        var queryString = url.ParseQueryString();
        return queryString["v"];
    }
    
    public static Dictionary<string, string> ParseQueryString(this Uri query)
    {
        return query.Query.Replace("?", "").Split('&').ToDictionary(pair => pair.Split('=').First(), pair => pair.Split('=').Last());
    }
    
    public static async Task<string> GetThumbnailUrl(this LavalinkTrack video)
    {
        var url = video.Uri;
        var queryString = url.ParseQueryString();
        string d = $"https://img.youtube.com/vi/{queryString["v"]}/maxresdefault.jpg";

        using var client = new HttpClient();
        var response = await client.GetAsync(d);
        
        if (response.IsSuccessStatusCode)
        {
            return d;
        }
        else
        {
            return $"https://img.youtube.com/vi/{queryString["v"]}/default.jpg";
        }
    }
}
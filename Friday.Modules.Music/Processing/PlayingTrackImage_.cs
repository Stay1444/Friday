using System.Reflection;
using Friday.Common.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Friday.Modules.Music.Processing;

public class PlayingTrackImage_
{
    public string TrackName { get; set; } = "Unknown";
    public string ArtistName { get; set; } = "Unknown";
    public string? TrackImageUrl { get; set; }
    public string? TrackImagePath { get; set; }
    public int TrackDurationSeconds { get; set; } = 0;
    public int TrackPositionSeconds { get; set; } = 0;
    public bool Playing { get; set; } = false;
    public bool Repeat { get; set; } = false;
    public int Volume { get; set; } = 100;
    
    public async Task Process()
    {
        Image image = new Image<Rgba32>(1200,200);

        Image thumbnail;
        if (TrackImageUrl is not null)
        {
            // Download image
            using var client = new HttpClient();
            var imageBytes = await client.GetByteArrayAsync(TrackImageUrl);
            thumbnail = Image.Load(imageBytes);
        }else if (TrackImagePath is not null)
        {
            // Load image
            thumbnail = await Image.LoadAsync(TrackImagePath);
        }
        else
        {
            // Use default image. Read from resources
            Resource resource = Resource.Load("Resources/Images/default_thumbnail.png");
            thumbnail = await Image.LoadAsync(resource.GetStream());
        }
        
        image.Mutate(x => x.Fill(new Rgba32(37,39,44))); // Background
        
        thumbnail.Mutate(x => x.Resize(290,290)); // Resize thumbnail
        FontCollection fonts = new FontCollection();
        fonts.Add(Resource.Load("Resources/Fonts/Roboto-Regular.ttf").GetStream());
        var gothamLight = fonts.Get("Roboto");

        image.Mutate(x =>
        {
            x.DrawImage(thumbnail, new Point(5,5), 1);
            x.DrawText(TrackName, gothamLight.CreateFont(72), Color.White, new PointF(300,5));
        });
        await image.SaveAsync("test.png", new PngEncoder());
    }

    private async Task ProcessFrame()
    {
        
    }
}
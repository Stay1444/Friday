using System.Diagnostics;
using Friday.Common.Entities;
using Friday.Modules.Music.Enums;
using Friday.Modules.Music.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Friday.Modules.Music.Processing;

public class PlayingTrackImage
{
    private static Resource _defaultThumbnailResource = Resource.Load("Resources/Images/default_thumbnail.png");
    private static Resource _fontResource = Resource.Load("Resources/Fonts/Roboto-Regular.ttf");
    private static Resource _playIconResource = Resource.Load("Resources/Images/play_icon.png");
    private static Resource _pauseIconResource = Resource.Load("Resources/Images/pause_icon.png");
    private string _trackName;
    private readonly string _artistName;
    private readonly string? _trackImageUrl;    
    private readonly string? _trackImagePath;
    private readonly int _trackDurationSeconds;
    private readonly int _trackPositionSeconds;
    private readonly int _trackPositionsToRender;
    private readonly GuildMusicState _state;
    private readonly RepeatMode _repeat;
    private readonly int _volume;
    private GuildMusic _music;
    public PlayingTrackImage(GuildMusic music, int n)
    {
        _trackName = music.Playing!.Title;
        _artistName = music.Playing!.Author;
        _trackImageUrl = music.Playing!.GetThumbnailUrl().Result;
        _trackImagePath = null;
        _trackDurationSeconds = (int)music.Playing!.Length.TotalSeconds;
        _trackPositionSeconds = (int)music.Playing!.Position.TotalSeconds;
        _trackPositionsToRender = n;
        _state = music.State;
        _repeat = music.Repeat;
        _volume = music.Volume;
        _music = music;
    }

    public async Task<MemoryStream> ProcessAsync()
    {
        Image<Rgba32> albumImage = await DownloadThumbnail();

        Image<Rgba32> image = new Image<Rgba32>(1300, 400);
        if (albumImage.Height > albumImage.Width)
        {
            //Crop rectangle from center
            var cropRect = new Rectangle(0, albumImage.Height / 2 / 2, albumImage.Width, albumImage.Width);
            albumImage.Mutate(x => x.Crop(cropRect));
        }
        else if (albumImage.Width > albumImage.Height)
        {
            var cropRect = new Rectangle(albumImage.Width /2 /2, 0, albumImage.Height, albumImage.Height);
            albumImage.Mutate(x => x.Crop(cropRect));
        }
        albumImage.Mutate(x => { x.Resize(image.Height, image.Height); });
        
        var fontCollection = new FontCollection();
        fontCollection.Add(_fontResource.GetStream());

        if (_trackName.Length > 30)
        {
            _trackName = _trackName.Substring(0, 30) + "...";
        }
        
        var trackNameFont = fontCollection.Get("Roboto").CreateFont(72);
        while(TextMeasurer.Measure(_trackName, new TextOptions(trackNameFont)).Width > image.Width - albumImage.Width - 20)
        {
            trackNameFont = fontCollection.Get("Roboto").CreateFont(trackNameFont.Size - 1);
        }
        var artistNameFont = fontCollection.Get("Roboto").CreateFont(24);
        var trackDurationFont = fontCollection.Get("Roboto").CreateFont(35);

        var connection = _music.GetConnection();
        var positionInitial = (int) connection!.CurrentState.PlaybackPosition.TotalSeconds;
        var position = (int)connection!.CurrentState.PlaybackPosition.TotalSeconds;
        var artistTextH =TextMeasurer.Measure(_trackName, new TextOptions(trackNameFont)).Height + 15;
        
        var playImage = Image.Load(_state == GuildMusicState.Playing ? _pauseIconResource.GetStream() : _playIconResource.GetStream());
        playImage.Mutate(x => { x.Resize(80, 80); });
        
        while (position <= _trackDurationSeconds && position <= positionInitial + _trackPositionsToRender)
        {
            var frame = new Image<Rgba32>(image.Width, image.Height);
            frame.Frames[0].Metadata.GetGifMetadata().FrameDelay = 100; // 1 Second
            frame.Mutate(x =>
            {
                x.Fill(new Rgba32(37, 39, 44));
                x.DrawImage(albumImage, new Point(0, 0), 1);
                x.DrawText(_trackName, trackNameFont, Color.White, new PointF(albumImage.Width + 15, 15));
                x.DrawText(_artistName, artistNameFont, Color.White, new PointF(albumImage.Width + 15, artistTextH));
                float progress = (float)position / _trackDurationSeconds * 1.0f;
                int center = (image.Width - albumImage.Width) / 2 + albumImage.Width;
                int progressBarPosition = center - 300;
                x.DrawProgressBar(600, 35, new PointF(progressBarPosition, 230), progress, new Rgba32(66, 135, 245,255), new Rgba32(27,29,34,255));
                string positionText = $"{position / 60:D2}:{position % 60:D2}";
                string durationText = $"{_trackDurationSeconds / 60:D2}:{_trackDurationSeconds % 60:D2}";

                var positionTextMeasure = TextMeasurer.Measure(positionText, new TextOptions(trackDurationFont));
                x.DrawText($"{positionText}", trackDurationFont, Color.White,new PointF(progressBarPosition - 40 - positionTextMeasure.Width, 230));
                x.DrawText($"{durationText}", trackDurationFont, Color.White, new PointF(progressBarPosition + 600 + 40, 230));
                
                x.DrawImage(playImage, new Point(center - playImage.Width / 2, 290), 1);
            });

            image.Frames.AddFrame(frame.Frames[0]);
            position += 1;
        }

        var stream = new MemoryStream();
        GifEncoder encoder = new GifEncoder()
        {
            Quantizer = new OctreeQuantizer(new QuantizerOptions
            {
                DitherScale = 0,
                Dither = null
            })
        };

        await image.SaveAsGifAsync(stream, encoder);
        return stream;
    }

    private async Task<Image<Rgba32>> DownloadThumbnail()
    {
        try
        {
            if (_trackImageUrl is not null)
            {
                var img =await NetworkImage.DownloadImage(_trackImageUrl);
                return img;
            }else if (_trackImagePath is not null)
            {
                return Image.Load<Rgba32>(_trackImagePath);
            }
            else
            {
                return Image.Load<Rgba32>(_defaultThumbnailResource.GetStream());
            }
        }catch
        {
            return Image.Load<Rgba32>(_defaultThumbnailResource.GetStream());
        }
    }

}
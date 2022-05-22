using Friday.Common.Entities;
using Friday.Modules.MiniGames.Games;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Friday.Modules.MiniGames.Images;

public class _2080Renderer
{
    private const int BOARD_SIZE = 4;
    private const int RESOLUTION = 600;
    private const int PADDING = 12;
    private const int CELL_SIZE = (RESOLUTION - (BOARD_SIZE + 1) * PADDING) / BOARD_SIZE;

    private static Dictionary<int, Rgba32> _colorMapping = new ()
    {
        { 2, Rgba32.ParseHex("#EEE4DA") },
        { 4, Rgba32.ParseHex("#EEE0C9") },
        { 8, Rgba32.ParseHex("#F3B27A") },
        { 16, Rgba32.ParseHex("#F69664") },
        { 32, Rgba32.ParseHex("#F65E3B") },
        { 64, Rgba32.ParseHex("#EDCF72") },
        { 128, Rgba32.ParseHex("#EDCC61") },
        { 256, Rgba32.ParseHex("#EDC850") },
        { 512, Rgba32.ParseHex("#EDC53F") },
        { 1024, Rgba32.ParseHex("#EDC22E") },
        { 2048, Rgba32.ParseHex("#EDC22E") },
    };

    private static Rgba32 _backgroundColor = Rgba32.ParseHex("#CDC1B3");
    private static Rgba32 _paddingColor = Rgba32.ParseHex("#BBAEA0");
    private static Resource _fontResource = Resource.Load("Resources/Fonts/ClearSans-Bold.ttf");
    private static FontFamily _fontFamily = new FontCollection().Add(_fontResource.GetStream());

    public static async Task<Stream> Render(_2048Game game)
    {
        var image = new Image<Rgba32>(RESOLUTION, RESOLUTION);
        
        var cellTasks = new Dictionary<Task<Image<Rgba32>>, Point>();
        
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                var cellValue = game.Board[x, y];

                var xPos = x * CELL_SIZE + PADDING + x * PADDING;
                var yPos = y * CELL_SIZE + PADDING + y * PADDING;

                var cellTask = GenerateCell(cellValue);
                cellTasks.Add(cellTask, new Point(xPos, yPos));
            }
        }
        
        await Task.WhenAll(cellTasks.Keys);
        
        image.Mutate(mut =>
        {
            mut.Fill(_paddingColor);

            foreach (var cellTask in cellTasks)
            {
                mut.DrawImage(cellTask.Key.Result, cellTask.Value, 1f);
            }
        });
        
        var stream = new MemoryStream();
        image.SaveAsPng(stream);
        stream.Position = 0;
        return stream;
    }

    private static Task<Image<Rgba32>> GenerateCell(int value)
    {
        var cell = new Image<Rgba32>(CELL_SIZE, CELL_SIZE);

        if (value == 0)
        {
            cell.Mutate(mut => mut.Fill(_backgroundColor));
            return Task.FromResult(cell);
        }
        
        cell.Mutate(mutate =>
        {
            mutate.Fill(_colorMapping[value]);
            
            int fontSize = (int) (CELL_SIZE / 1.5f);
            var font = _fontFamily.CreateFont(fontSize);
            var color = Color.WhiteSmoke;
            if (value <= 4)
            {
                color = Color.Gray;
            }

            var measures = TextMeasurer.Measure(value.ToString(), new TextOptions(font));
            while (measures.Width > CELL_SIZE - PADDING|| measures.Height > CELL_SIZE - PADDING)
            {
                fontSize--;
                font = _fontFamily.CreateFont(fontSize);
                measures = TextMeasurer.Measure(value.ToString(), new TextOptions(font));
            }
            var position = new PointF((CELL_SIZE - measures.Width) / 2, (CELL_SIZE - measures.Height) / 2);
            mutate.DrawText(value.ToString(), font, color, position);
        });
        
        return Task.FromResult(cell);
    }
}
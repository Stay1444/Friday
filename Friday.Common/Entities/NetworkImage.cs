using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Friday.Common.Entities;

public static class NetworkImage
{
    private static readonly Dictionary<string,Guid> UrlCache = new();
    
    public static async Task<Image<Rgba32>> DownloadImage(string url)
    {
        if (UrlCache.TryGetValue(url, out var id))
        {
            return Image.Load<Rgba32>(Path.Combine(".tmp", id.ToString()));
        }

        await using var r = await DownloadStream(url);
        r.Position = 0;
        return Image.Load<Rgba32>(r);
    }

    public static async Task<Stream> DownloadStream(string url)
    {
        if (UrlCache.ContainsKey(url))
        {
            await using var fs = new FileStream(UrlCache[url].ToString(), FileMode.Open);
            
            var ms = new MemoryStream();
            await fs.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        }

        if (!Directory.Exists(".tmp"))
        {
            Directory.CreateDirectory(".tmp");
        }
        
        using var client = new HttpClient();
        var responseStream = await client.GetStreamAsync(url);
        var fileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(".tmp", fileName);
        var fileStream = new FileStream(filePath, FileMode.Create);
        await responseStream.CopyToAsync(fileStream);
        UrlCache.Add(url, Guid.Parse(fileName));
        return fileStream;
    }
}
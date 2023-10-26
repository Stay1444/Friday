
using System.Diagnostics;
using SimpleCDN.Wrapper;
using SixLabors.ImageSharp.Formats.Png;

namespace Friday.Modules.Minesprout.Services;

public class ServerBannerResolver
{
    private readonly SimpleCdnClient _cdnClient;
    private Dictionary<int, Guid> _cache = new Dictionary<int, Guid>();
    private SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1);
    private DateTime _lastCacheInvalidation = DateTime.UtcNow;
    private TimeSpan _cacheLifespan = TimeSpan.FromHours(1);
    private HttpClient _httpClient = new HttpClient();
    
    internal ServerBannerResolver(SimpleCdnClient cdnClient)
    {
        this._cdnClient = cdnClient;
    }

    private async Task RunFfmpeg(string directory, string input, string output)
    {
        var p0 = new ProcessStartInfo()
        {
            WorkingDirectory = directory,
            FileName = "ffmpeg",
            Arguments = $"-y -i {input} -vf palettegen palette%03d.png"
        };

        await Process.Start(p0)!.WaitForExitAsync();

        var p1 = new ProcessStartInfo()
        {
            WorkingDirectory = directory,
            FileName = "ffmpeg",
            Arguments = $"-y -i {input} -i palette%03d.png -filter_complex paletteuse -r 10 {output}"
        };

        await Process.Start(p1)!.WaitForExitAsync();
    }
    
    public async Task<string> ResolveAsync(string bannerUrl, int serverId)
    {
        try
        {
            await _cacheSemaphore.WaitAsync();
            
            if (_lastCacheInvalidation + _cacheLifespan < DateTime.UtcNow)
            {
                _cache.Clear();
            }

            if (_cache.TryGetValue(serverId, out var value))
            {
                return $"{_cdnClient.Host}{value}/{serverId}.png";
            }

            var convId = Guid.NewGuid();
            try
            {
                Directory.CreateDirectory(convId.ToString());
                {
                    await using var file = File.OpenWrite($"{convId}/banner.webm");
                    var httpStream = await _httpClient.GetStreamAsync(bannerUrl);
                    await httpStream.CopyToAsync(file);
                }
                await RunFfmpeg($"{convId}", "banner.webm", "banner.gif");

                await using var readFile = File.OpenRead($"{convId}/banner.gif");
                var id = await _cdnClient.UploadAsync($"{serverId}.gif", readFile, false,
                    DateTime.UtcNow + TimeSpan.FromHours(8));
                _cache.Add(serverId, id);
                return $"{_cdnClient.Host}{id}/{serverId}.gif";
            }
            finally
            {
                Directory.Delete($"{convId}", true);
            }
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }
}
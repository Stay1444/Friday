
using SimpleCDN.Wrapper;
using SixLabors.ImageSharp.Formats.Png;

namespace Friday.Modules.Minesprout.Services;

public class ServerIconResolver
{
    private readonly SimpleCdnClient _cdnClient;
    private Dictionary<int, Guid> _cache = new Dictionary<int, Guid>();
    private SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1);
    private DateTime _lastCacheInvalidation = DateTime.UtcNow;
    private TimeSpan _cacheLifespan = TimeSpan.FromHours(1);
    internal ServerIconResolver(SimpleCdnClient cdnClient)
    {
        this._cdnClient = cdnClient;
    }
    
    public async Task<string> ResolveAsync(string base64, int serverId)
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

            var imageBytes = Convert.FromBase64String(base64.Replace("data:image/png;base64,", ""));
            var stream = new MemoryStream();
            stream.Write(imageBytes);
            stream.Position = 0;
            var id = await _cdnClient.UploadAsync($"{serverId}.png", stream, false,
                DateTime.UtcNow + TimeSpan.FromHours(8));
            
            _cache.Add(serverId, id);
            return $"{_cdnClient.Host}{id}/{serverId}.png";
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }
}
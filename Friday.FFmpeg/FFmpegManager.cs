using System.Diagnostics;

namespace Friday.FFmpeg;

public class FFmpegManager : IDisposable
{
    private Dictionary<int, FFmpegInstance> _instances = new Dictionary<int, FFmpegInstance>();
    private readonly string _path;
    public FFmpegManager(string ffmpegPath)
    {
        if (!File.Exists(ffmpegPath)) throw new FileNotFoundException(ffmpegPath);
        this._path = ffmpegPath;
    }

    internal void OnDispose(FFmpegInstance instance)
    {
        lock (_instances)
        {
            _instances.Remove(instance.Pid);
        }
    }
    
    public FFmpegInstance NewInstance()
    {
        Process? process;
        try
        {
            process = Process.Start(new ProcessStartInfo()
            {
                FileName = _path,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                
                Arguments = $"-nostats -loglevel 0 -i pipe: -ac 2 -f s16le -ar 48000 pipe:"
            });
        }
        catch(Exception error)
        {
            Console.WriteLine(error);
            throw;
        }

        if (process is null)
        {
            throw new NullReferenceException(nameof(process));
        }

        var instance = new FFmpegInstance(this, process);
        
        lock (_instances)
        {
            _instances.Add(instance.Pid, instance);
        }

        return instance;
    }

    public void Dispose()
    {
        lock (_instances)
        {
            foreach (var fFmpegInstance in _instances)
            {
                fFmpegInstance.Value.Dispose();
            }
        }
    }
}
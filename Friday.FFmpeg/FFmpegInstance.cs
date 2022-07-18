using System.Diagnostics;

namespace Friday.FFmpeg;

public class FFmpegInstance : IDisposable, IAsyncDisposable
{
    private const int BUFFER_SIZE = 1024;
    
    private readonly FFmpegManager _manager;
    
    private readonly Process _process;

    private readonly SemaphoreSlim _readSync = new SemaphoreSlim(1);
    private readonly SemaphoreSlim _writeSync = new SemaphoreSlim(1);

    private Stream _stdin;
    private Stream _stdout;
    
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public int Pid { get; }

    internal FFmpegInstance(FFmpegManager manager, Process process)
    {
        this._process = process;
        this._manager = manager;
        this.Pid = process.Id;
        this._stdin = process.StandardInput.BaseStream;
        this._stdout = process.StandardOutput.BaseStream;

        _process.Exited += (_, _) =>
        {
            Dispose();
        };
    }

    public async Task<byte[]> ReadAsync(CancellationToken token)
    {
        try
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);

            await _readSync.WaitAsync(linked.Token);
            
            var buff = new byte[BUFFER_SIZE];
            var read = await _stdout.ReadAsync(buff, 0, buff.Length, linked.Token);

            if (read == 0)
            {
                await DisposeAsync();
                return Array.Empty<byte>();
            }

            var result = new byte[read];
            Array.Copy(buff, 0, result, 0, read);
            return result;
        }
        catch
        {
            await DisposeAsync();
        }
        finally
        {
            _readSync.Release();
        }
        return Array.Empty<byte>();
    }
    
    public Task<byte[]> ReadAsync()
    {
        return ReadAsync(CancellationToken.None);
    }

    public async Task WriteAsync(byte[] buff, int count, CancellationToken token)
    {
        try
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            
            await _writeSync.WaitAsync(linked.Token);

            await _stdin.WriteAsync(buff, 0, count, token);
        }
        catch
        {
            await DisposeAsync();
        }
        finally
        {
            _writeSync.Release();
        }
    }

    public Task WriteAsync(byte[] buff, CancellationToken token)
    {
        return WriteAsync(buff, buff.Length, token);
    }

    public Task WriteAsync(byte[] buff)
    {
        return WriteAsync(buff, CancellationToken.None);
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _manager.OnDispose(this);
        _process.Kill();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _manager.OnDispose(this);
        _process.Kill();
        await _process.WaitForExitAsync();
    }
}
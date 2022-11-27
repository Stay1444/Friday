using DSharpPlus.Lavalink;

namespace Friday.Modules.Music.Models;

public class MusicQueue
{
    private List<LavalinkTrack> _tracks = new ();
    private List<LavalinkTrack> _played = new ();
    public int Count => _tracks.Count;
    
    public LavalinkTrack? Dequeue()
    {
        var track = _tracks.FirstOrDefault();
        if (track == null) return null;
        _played.Add(track);
        _tracks.Remove(track);
        return track;
    }
    
    public void Enqueue(LavalinkTrack track)
    {
        Console.WriteLine("Enqueued " + track.Title);
        _tracks.Add(track);
    }
    
    public void Clear()
    {
        _tracks.Clear();
    }
    
    public LavalinkTrack? Peek()
    {
        return _tracks.FirstOrDefault();
    }
    
    public bool Contains(LavalinkTrack track)
    {
        return _tracks.Contains(track);
    }
    
    public LavalinkTrack? ShuffledDequeue()
    {
        if (_tracks.Count == 0)
            return null;
        
        int randomTrackIndex = new Random().Next(0, _tracks.Count);
        var randomTrack = _tracks[randomTrackIndex];
        _tracks.RemoveAt(randomTrackIndex);
        _played.Add(randomTrack);
        return randomTrack;
    }
    
    public List<LavalinkTrack> ToList()
    {
        return _tracks.ToList();
    }
    
    public IReadOnlyList<LavalinkTrack> ToReadOnlyList()
    {
        return _tracks.ToList();
    }

    public void Reset()
    {
        _tracks.AddRange(_played);
        _played.Clear();
    }
    
    public LavalinkTrack? Previous()
    {
        if (_played.Count == 0)
            return null;
        
        var track = _played.LastOrDefault();
        if (track == null)
            return null;
        
        _played.Remove(track);
        return track;
    }
}
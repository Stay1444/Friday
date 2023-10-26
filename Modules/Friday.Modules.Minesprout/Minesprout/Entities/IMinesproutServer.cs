namespace Friday.Modules.Minesprout.Minesprout.Entities;

public interface IMinesproutServer
{
    public string? Name { get; }
    public string? Description { get; }
    public MinesproutServerStatus? Status { get; }
    public string Ip { get; }
    public int Id { get; }
    public string MaxVersion { get; }
    public string MinVersion { get; }
    public string MainMode { get; }
    public string Type { get; }
    public string Country { get; }

    public string? Intro { get; }
    public string? Website { get; }
}
using System.Text.Json.Serialization;

namespace Friday.Modules.Minesprout.Minesprout.Entities;

public class MinesproutServer : IMinesproutServer
{
    public class MinesproutServerData
    {
        public string Ip { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
        public string MainMode { get; set; }
        public string? Name { get; set; }
        public int Id { get; set; }
        public string Country { get; set; }
        public string Type { get; set; }
        public string? Intro { get; set; }
        public string? Website { get; set; }
    }

    public MinesproutServerStatus? Status { get; set; }
    public MinesproutServerData ServerData { get; set; }
    [JsonIgnore] public string? Name => ServerData.Name;
    public string? Description => ServerData.Description;
    public string Ip => ServerData.Ip;
    public int Id => ServerData.Id;
    public string MaxVersion { get; set; }
    public string MinVersion { get; }
    public string MainMode => ServerData.MainMode;
    public string Type => ServerData.Type;
    public string Country => ServerData.Country;

    public string? Intro => ServerData?.Intro;

    public string? Website => ServerData?.Website;
}
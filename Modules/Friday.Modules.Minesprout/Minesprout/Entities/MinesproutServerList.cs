namespace Friday.Modules.Minesprout.Minesprout.Entities;

public class MinesproutServerList
{
    public required List<MinesproutServerListing> Servers { get; set; }
    public int TotalCount { get; set; }
}
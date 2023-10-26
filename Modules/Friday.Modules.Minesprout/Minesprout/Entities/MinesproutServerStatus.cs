namespace Friday.Modules.Minesprout.Minesprout.Entities;

public class MinesproutServerStatusPlayers
{
    public int Online { get; set; }
    public int Max { get; set; }
}

public class MinesproutServerStatus
{
    public string? Favicon { get; set; }
    public MinesproutServerStatusPlayers? Players { get; set; }
}
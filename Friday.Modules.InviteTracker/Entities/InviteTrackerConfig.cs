namespace Friday.Modules.InviteTracker.Entities;

public class InviteTrackerConfig
{
    public bool Enabled { get; set; } = false;

    public ulong JoinLogChannel { get; set; }
}
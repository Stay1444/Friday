
namespace Friday.Modules.ReactionRoles.Entities;

public class ReactionRole
{
    public ulong Id { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
    public List<ulong> RoleIds { get; set; } = new ();
    public ReactionRoleBehaviour Behaviour { get; set; } = ReactionRoleBehaviour.Toggle;
    public string? Emoji { get; set; }
    public string? ButtonId { get; set; }
    public bool SendMessage { get; set; }
    public string? Warning { get; set; }
}

public enum ReactionRoleBehaviour
{
    Add,
    Remove,
    Toggle
}


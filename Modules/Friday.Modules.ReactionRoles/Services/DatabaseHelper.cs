using System.Data.Common;
using Friday.Common;
using Friday.Common.Services;
using Friday.Modules.ReactionRoles.Entities;

namespace Friday.Modules.ReactionRoles.Services;

public class DatabaseHelper
{
    private DatabaseProvider _db;

    public DatabaseHelper(DatabaseProvider db)
    {
        _db = db;
    }

    private async Task<IEnumerable<ReactionRole>> ReadList(DbDataReader reader)
    {
        var result = new List<ReactionRole>();
        while (await reader.ReadAsync())
        {
            result.Add(new ReactionRole()
            {
                Id = (ulong) reader.GetInt64(0),
                ChannelId = (ulong) reader.GetInt64(2),
                MessageId = (ulong) reader.GetInt64(3),
                RoleIds = reader.GetString(4).Split(',').Select(ulong.Parse).ToList(),
                Behaviour = (ReactionRoleBehaviour) reader.GetByte(5),
                Emoji = reader.ReadNullableString(6),
                ButtonId = reader.ReadNullableString(7),
                SendMessage = reader.GetBoolean(8)
            });
        }
        
        return result;
    }

    public async Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(ulong guildId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM rr_reaction_roles WHERE guild_id = @guildId";
        command.Parameters.AddWithValue("@guildId", guildId);
        await using var reader = await command.ExecuteReaderAsync();
        return await ReadList(reader);
    }

    public async Task<IEnumerable<ReactionRole>> GetReactionRolesAsync(ulong guildId, ulong channelId, ulong messageId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM rr_reaction_roles WHERE guild_id = @guildId AND channel_id = @channelId AND message_id = @messageId";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@channelId", channelId);
        command.Parameters.AddWithValue("@messageId", messageId);
        await using var reader = await command.ExecuteReaderAsync();
        return await ReadList(reader);
    }

    public async Task InsertReactionRoleAsync(ulong guildId, ulong channelId, ulong messageId,
        ReactionRole reactionRole)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO rr_reaction_roles (guild_id, channel_id, message_id, role_ids, behaviour, emoji, button_id, send_message) VALUES (@guildId, @channelId, @messageId, @roleIds, @behaviour, @emoji, @buttonId, @sendMessage)";
        command.Parameters.AddWithValue("@guildId", guildId);
        command.Parameters.AddWithValue("@channelId", channelId);
        command.Parameters.AddWithValue("@messageId", messageId);
        command.Parameters.AddWithValue("@roleIds", string.Join(",", reactionRole.RoleIds));
        command.Parameters.AddWithValue("@behaviour", (int) reactionRole.Behaviour);
        command.Parameters.AddWithValue("@emoji", reactionRole.Emoji);
        command.Parameters.AddWithValue("@buttonId", reactionRole.ButtonId);
        command.Parameters.AddWithValue("@sendMessage", reactionRole.SendMessage);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateReactionRoleAsync(ReactionRole reactionRole)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE rr_reaction_roles SET role_ids=@roleIds, behaviour=@behaviour, emoji=@emoji, button_id=@buttonId, send_message=@sendMessage WHERE id=@id";

        command.Parameters.AddWithValue("@id", reactionRole.Id);
        command.Parameters.AddWithValue("@roleIds", string.Join(",", reactionRole.RoleIds));
        command.Parameters.AddWithValue("@behaviour", (int) reactionRole.Behaviour);
        command.Parameters.AddWithValue("@emoji", reactionRole.Emoji);
        command.Parameters.AddWithValue("@buttonId", reactionRole.ButtonId);
        command.Parameters.AddWithValue("@sendMessage", reactionRole.SendMessage);
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteReactionRoleAsync(ulong id)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM rr_reaction_roles WHERE id=@id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<ReactionRole?> GetReactionRoleAsync(ulong id)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM rr_reaction_roles WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        var result = await ReadList(reader);
        return result.FirstOrDefault();
    }

}
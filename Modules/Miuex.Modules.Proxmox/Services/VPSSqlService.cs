using Friday.Common.Services;

namespace Miuex.Modules.Proxmox.Services;

public class VPSSqlService
{
    private DatabaseProvider _provider;
    public VPSSqlService(DatabaseProvider provider)
    {
        this._provider = provider;
    }

    
    
    //get vps by user and name
    public async Task<(ulong owner, int nodeId, int vmId, string name)?> GetVps(string name)
    {
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT nodeId, vmId, name, userId FROM vps WHERE name = @name";
        cmd.Parameters.AddWithValue("@name", name);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        long userId = (long) reader["userId"];
        
        return (
            (ulong) userId,
            (int)reader["nodeId"],
            (int)reader["vmId"],
            reader["name"].ToString() ?? ""
        );
    }
    
    public async Task<List<(int nodeId, int vmId, string name)>> GetBindedVps(ulong user)
    {
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT nodeId, vmId, name FROM vps WHERE userId = @user";
        cmd.Parameters.AddWithValue("@user", user);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<(int nodeId, int vmId, string name)>();
        while (await reader.ReadAsync())
        {
            result.Add((
                (int)reader["nodeId"],
                (int)reader["vmId"],
                reader["name"].ToString() ?? ""
            ));
        }
        
        return result;
    }
    
    
    // add vps
    public async Task AddVps(ulong user, int nodeId, int vmId, string name)
    {
        if (name.Length > 64)
        {
            return;
        }
        
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO vps (userId, nodeId, vmId, name) VALUES (@user, @nodeId, @vmId, @name)";
        cmd.Parameters.AddWithValue("@user", user);
        cmd.Parameters.AddWithValue("@nodeId", nodeId);
        cmd.Parameters.AddWithValue("@vmId", vmId);
        cmd.Parameters.AddWithValue("@name", name);
        
        await cmd.ExecuteNonQueryAsync();
    }
    
    //rename vps using user id, and vmId, new name
    public async Task<bool> RenameVps(ulong user, int nodeId, int vmId, string name)
    {

        if (name.Length > 64)
        {
            return false;
        }
        
        if (await GetVps(name) is not null)
        {
            return false; // name already exists
        }
        
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE vps SET name = @name WHERE userId = @user AND nodeId = @nodeId AND vmId = @vmId";
        cmd.Parameters.AddWithValue("@user", user);
        cmd.Parameters.AddWithValue("@nodeId", nodeId);
        cmd.Parameters.AddWithValue("@vmId", vmId);
        cmd.Parameters.AddWithValue("@name", name);
        
        await cmd.ExecuteNonQueryAsync();
        
        return true;
    }
    
    //remove vps using userId, vmId, nodeId
    public async Task RemoveVps(string name)
    {
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM vps WHERE name = @name";
        cmd.Parameters.AddWithValue("@name", name);        
        await cmd.ExecuteNonQueryAsync();
    }
    
    //Get all vps from all users
    
    public async Task<List<(ulong user, int nodeId, int vmId, string name)>> GetAllVps()
    {
        await using var conn = _provider.GetConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT userId, nodeId, vmId, name FROM vps";
        
        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<(ulong user, int nodeId, int vmId, string name)>();
        while (await reader.ReadAsync())
        {
            long userId = (long) reader["userId"];
            result.Add((
                (ulong)userId,
                (int)reader["nodeId"],
                (int)reader["vmId"],
                reader["name"].ToString() ?? ""
            ));
        }
        
        return result;
    }

}
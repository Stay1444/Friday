using Friday.Common.Services;

namespace Friday.Modules.Birthday.Services;

public class DatabaseService
{
    private DatabaseProvider _db;

    public DatabaseService(DatabaseProvider db)
    {
        _db = db;
    }

    public async Task<bool> HasBirthdayAsync(ulong userId)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM birthday_dates WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task<Entities.Birthday> GetBirthdayAsync(ulong userId)
    {
        if (!await HasBirthdayAsync(userId))
            throw new Exception("User has no birthday set");
        
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT date, public FROM birthday_dates WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new Entities.Birthday(userId, reader.GetDateTime(0))
        {
            Public = reader.GetBoolean(1)
        };
    }

    public async Task InsertBirthdayAsync(Entities.Birthday birthday)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO birthday_dates (id, date, public) VALUES (@userId, @date, @public)";
        command.Parameters.AddWithValue("@userId", birthday.Id);
        command.Parameters.AddWithValue("@date", birthday.BirthdayDate);
        command.Parameters.AddWithValue("@public", birthday.Public);
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateBirthdayAsync(Entities.Birthday birthday)
    {
        await using var connection = _db.GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE birthday_dates SET date = @date, public = @public WHERE id = @userId";
        command.Parameters.AddWithValue("@userId", birthday.Id);
        command.Parameters.AddWithValue("@date", birthday.BirthdayDate);
        command.Parameters.AddWithValue("@public", birthday.Public);
        await command.ExecuteNonQueryAsync();
    }
}
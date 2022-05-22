using Friday.Common.Services;

namespace Friday.Modules.CustomCommands.Services;

public class DatabaseService
{
    private DatabaseProvider _db;
    public DatabaseService(DatabaseProvider provider)
    {
        this._db = provider;
    }
}
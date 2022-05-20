namespace Friday.Modules.Backups.Entities;

public class Configuration
{
    public string CdnHost { get; set; } = "";
    public string ApiKey { get; set; } = Guid.Empty.ToString();
}
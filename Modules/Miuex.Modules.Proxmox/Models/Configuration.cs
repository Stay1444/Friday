namespace Miuex.Modules.Proxmox.Models;

public class Configuration
{
    public string? ConnectionString { get; set; }
    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? Token { get; set; }
    public ulong ServerId { get; set; }
    
    public ulong? LogsChannel { get; set; }
    public ulong? AdminRole { get; set; }
}
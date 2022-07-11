using System.Text.Json.Serialization;

namespace Miuex.Modules.Proxmox.Models;

public class VirtualMachine
{
    /// <summary>
    /// Virtual Machine ID.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Virtual Machine Node ID.
    /// </summary>
    public int Node { get; set; }
    
    /// <summary>
    /// Virtual Machine Node Name.
    /// </summary>
    public string? NodeName { get; set; }
    
    /// <summary>
    /// Name of the Virtual Machine.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Status of the Virtual Machine.
    /// </summary>
    public PveStatus Status { get; set; } = PveStatus.Unknown;
 
    /// <summary>
    /// Process ID of the Virtual Machine. -1 if not running.
    /// </summary>
    public int ProcessId { get; set; }
    
    /// <summary>
    /// Resources used by the Virtual Machine.
    /// </summary>
    public Resources? Resources { get; set; }
    
    /// <summary>
    /// Uptime in seconds
    /// </summary>
    public long UptimeSeconds { get; set; } = 0;
    
    /// <summary>
    /// Uptime
    /// </summary>
    [JsonPropertyName("Uptime")]
    public string UptimeStr
    {
        get
        {
            return TimeSpan.FromSeconds(UptimeSeconds).ToString();
        }
    }
    
    [JsonIgnore]
    public TimeSpan Uptime
    {
        get
        {
            return TimeSpan.FromSeconds(UptimeSeconds);
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Miuex.Modules.Proxmox.Models;

public class Node
{
    /// <summary>
    /// The unique identifier of the node.
    /// </summary>
    [Required]
    public int Id { get; set; }
    /// <summary>
    /// Node name.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the node.
    /// </summary>
    public PveStatus Status { get; set; } = PveStatus.Unknown;
    
    
    /// <summary>
    /// API Endpoint of the node.
    /// </summary>
    [JsonIgnore]
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// API Key of the node.
    /// </summary>
    [JsonIgnore]
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// VM Count of the node.
    /// </summary>
    public int VmCount { get; set; } = 0;

    /// <summary>
    /// Resources of the node.
    /// </summary>
    public Resources? Resources { get; set; } = null;

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
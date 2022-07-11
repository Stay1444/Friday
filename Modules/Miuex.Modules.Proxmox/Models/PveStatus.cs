using System.Text.Json.Serialization;

namespace Miuex.Modules.Proxmox.Models;

[Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
public enum PveStatus
{
    /// <summary>
    /// The status is unknown
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Stopped
    /// </summary>
    Stopped,
    /// <summary>
    /// Running
    /// </summary>
    Running,
    
    /// <summary>
    /// Only used  for VMs
    /// </summary>
    Paused,
}
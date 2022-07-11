using DSharpPlus;
using Miuex.Modules.Proxmox.Models;

namespace Miuex.Modules.Proxmox;

public class Constants
{
    public const int COLOR_DEFAULT = 0x19d1ce;
    
    public static DateTime StartTime = DateTime.MinValue;
    
    public static DiscordClient? Instance { get; set; }
    public static Configuration? Config { get; set; }
}
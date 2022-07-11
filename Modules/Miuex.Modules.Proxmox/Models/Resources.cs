namespace Miuex.Modules.Proxmox.Models;

public class Resources
{
    /// <summary>
    ///     Current CPU usage
    /// </summary>
    public double Cpu { get; set; }

    /// <summary>
    ///     Memory Capacity in bytes
    /// </summary>
    public long TotalMemory { get; set; }

    /// <summary>
    ///     Used Memory in bytes
    /// </summary>
    public long UsedMemory { get; set; }

    /// <summary>
    ///     Current Memory usage percentage
    /// </summary>
    public double Memory
    {
        get
        {
            try
            {
                return (double) UsedMemory / TotalMemory;
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }
    }

    /// <summary>
    ///     Disk Capacity in bytes
    /// </summary>
    public long TotalDisk { get; set; }

    /// <summary>
    ///     Used Disk space in bytes. Only available on Nodes
    /// </summary>
    public long UsedDisk { get; set; }

    /// <summary>
    ///     Free Disk space in bytes
    /// </summary>
    public long FreeDisk => TotalDisk - UsedDisk;

    /// <summary>
    ///     Available Disk space in bytes. Only available on Nodes
    /// </summary>
    public long AvailableDisk { get; set; }

    /// <summary>
    ///     Disk usage percentage. Only available on Nodes
    /// </summary>
    public double Disk
    {
        get
        {
            try
            {
                return (double) UsedDisk / TotalDisk;
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }
    }
}
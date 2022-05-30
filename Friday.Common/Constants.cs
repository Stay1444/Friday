namespace Friday.Common;

public class Constants
{
    public const string Version = "1.0.0";
    public static DateTime ProcessStartTimeUtc { get; } = DateTime.UtcNow;
    public static char[] AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
}
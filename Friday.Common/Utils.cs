using Emzi0767;

namespace Friday.Common;

public static class Utils
{
    public static IEnumerable<String> SplitInParts(this String s, Int32 partLength) {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (partLength <= 0)
            throw new ArgumentException("Part length has to be positive.", nameof(partLength));

        for (var i = 0; i < s.Length; i += partLength)
            yield return s.Substring(i, Math.Min(partLength, s.Length - i));
    }
    
    public static TimeSpan ParseVulgarTimeSpan(string time)
    {
        // Format: "1d2h3m4s"
        
        var result = TimeSpan.Zero; 
        var validUnits = new[]{'y','w','d', 'h', 'm', 's'};

        for (int i = 0; i < time.Length; i++)
        {
            char c = time[i];
            //check if c is a number
            if (c.IsBasicDigit()) continue;
            
            if (!validUnits.Contains(c))
            {
                throw new ArgumentException("Invalid time unit: " + c);
            }

            //Create a number using all the digits charaters before the current char
            string numberString = time.Substring(0, i);
            int number = int.Parse(numberString);
            
            switch (c)
            {
                case 'y':
                    result += TimeSpan.FromDays(number * 365);
                    break;
                case 'w':
                    result += TimeSpan.FromDays(number * 7);
                    break;
                case 'd':
                    result += TimeSpan.FromDays(number);
                    break;
                case 'h':
                    result += TimeSpan.FromHours(number);
                    break;
                case 'm':
                    result += TimeSpan.FromMinutes(number);
                    break;
                case 's':
                    result += TimeSpan.FromSeconds(number);
                    break;
                default:
                    throw new ArgumentException("Invalid time unit: " + c);
            }
        }
        
        return result;
    }
}
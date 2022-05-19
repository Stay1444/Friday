namespace Friday.Common.Entities;

public struct HumanTimeSpan
{
    public TimeSpan Value { get; set; }
    
    public HumanTimeSpan(TimeSpan value)
    {
        Value = value;
    }
    
    public static implicit operator TimeSpan(HumanTimeSpan humanTimeSpan)
    {
        return humanTimeSpan.Value;
    }
    
    public static implicit operator HumanTimeSpan(TimeSpan timeSpan)
    {
        return new HumanTimeSpan {Value = timeSpan};
    }

    public static HumanTimeSpan Parse(string time)
    {
        var keyChars = new[] {'d', 'h', 'm', 's'};
        var result = new HumanTimeSpan();
        var buffer = new List<char>();
        foreach (var ch in time)
        {
            if (char.IsNumber(ch))
            {
                buffer.Add(ch);
                continue;
            }
            
            if (buffer.Count == 0)
            {
                throw new ArgumentException("Invalid time format");
            }
            
            var value = int.Parse(new string(buffer.ToArray()));           
            
            int index = Array.IndexOf(keyChars, ch);
            
            if (index == -1)
            {
                throw new ArgumentException("Invalid time format");
            }
            
            keyChars[index] = '\0';
            
            //reversed for loop
            
            for (int i = keyChars.Length - 1; i >= 0; i--)
            {
                if (keyChars[i] == '\0' && i > index)
                {
                    throw new ArgumentException("Invalid time format");
                }
            }
            
            buffer.Clear();
            
            switch (ch)
            {
                case 'd':
                    result.Value += TimeSpan.FromDays(value);
                    break;
                case 'h':
                    result.Value += TimeSpan.FromHours(value);
                    break;
                case 'm':
                    result.Value += TimeSpan.FromMinutes(value);
                    break;
                case 's':
                    result.Value += TimeSpan.FromSeconds(value);
                    break;
            }
            
        }
        
        return result;
    }

    public string ToHumanTime()
    {
        string result = "";
        
        if (Value.Days > 0)
        {
            result += Value.Days + "d";
        }
        
        if (Value.Hours > 0)
        {
            result += Value.Hours + "h";
        }
        
        if (Value.Minutes > 0)
        {
            result += Value.Minutes + "m";
        }
        
        if (Value.Seconds > 0)
        {
            result += Value.Seconds + "s";
        }
        
        return result;
    }
    
    public string Humanize(int precision = int.MaxValue)
    {
        var result = new List<string>();
        
        if (Value.Days > 0 && precision > 0)
        {
            result.Add(Value.Days + " day" + (Value.Days > 1 ? "s" : ""));
            precision--;
        }
        
        if (Value.Hours > 0 && precision > 0)
        {
            result.Add(Value.Hours + " hour" + (Value.Hours > 1 ? "s" : ""));
            precision--;
        }
        
        if (Value.Minutes > 0 && precision > 0)
        {
            result.Add(Value.Minutes + " minute" + (Value.Minutes > 1 ? "s" : ""));
            precision--;
        }
        
        if (Value.Seconds > 0 && precision > 0)
        {
            result.Add(Value.Seconds + " second" + (Value.Seconds > 1 ? "s" : ""));
            precision--;
        }
        
        return string.Join(" ", result);
    }
    
    public static bool TryParse(string time, out HumanTimeSpan result)
    {
        try
        {
            result = Parse(time);
            return true;
        }
        catch (Exception)
        {
            result = new HumanTimeSpan();
            return false;
        }
    }
    
    
    public static HumanTimeSpan operator +(HumanTimeSpan a, HumanTimeSpan b)
    {
        return new HumanTimeSpan(a.Value + b.Value);
    }
    
    public static HumanTimeSpan operator -(HumanTimeSpan a, HumanTimeSpan b)
    {
        return new HumanTimeSpan(a.Value - b.Value);
    }
    
    public static bool operator ==(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value == b.Value;
    }
    
    public static bool operator !=(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value != b.Value;
    }
    
    public static bool operator >(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value > b.Value;
    }
    
    public static bool operator <(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value < b.Value;
    }
    
    public static bool operator >=(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value >= b.Value;
    }
    
    public static bool operator <=(HumanTimeSpan a, HumanTimeSpan b)
    {
        return a.Value <= b.Value;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is HumanTimeSpan && Value == ((HumanTimeSpan) obj).Value;
    }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
}
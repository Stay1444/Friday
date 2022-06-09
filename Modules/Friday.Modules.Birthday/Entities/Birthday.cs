namespace Friday.Modules.Birthday.Entities;

public class Birthday
{
    public ulong Id { get; }
    public DateTime BirthdayDate { get; }
    public bool Public { get; set; } = true;
    
    public bool IsBirthday(DateTime date)
    {
        return BirthdayDate.Day == date.Day && BirthdayDate.Month == date.Month;
    }

    public bool IsBirthday()
    {
        return IsBirthday(DateTime.UtcNow);
    }
    
    public Birthday(ulong id, DateTime birthdayDate)
    {
        Id = id;
        BirthdayDate = birthdayDate;
    }

    public DateTime CalculateNextBirthday()
    {
        var now = DateTime.UtcNow;
        var nextBirthday = new DateTime(now.Year, BirthdayDate.Month, BirthdayDate.Day);
        if (nextBirthday < now)
            nextBirthday = nextBirthday.AddYears(1);
        return nextBirthday;
    }
}
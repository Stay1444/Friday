namespace Friday.Common.Attributes;

public class ColumnName : Attribute
{
    public string Name { get; private set; }
    public ColumnName(string name)
    {
        Name = name;
    }
}
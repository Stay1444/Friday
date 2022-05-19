namespace Friday.UI.Entities;

public class Ref<T>
{
    public T Value;
    
    public Ref(T value)
    {
        Value = value;
    }
}
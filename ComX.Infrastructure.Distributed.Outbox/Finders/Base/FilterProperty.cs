namespace ComX.Infrastructure.Distributed.Outbox;

public class FilterProperty<TProperty>
{
    public TProperty Value { get; private set; } = default;

    public bool HasValue { get; private set; } = false;

    public FilterProperty()
    {
    }

    public static FilterProperty<TProperty> SetValue(TProperty value)
    {
        return new FilterProperty<TProperty>
        {
            Value = value,
            HasValue = true
        };
    }

    public static FilterProperty<TProperty> SetNoValue()
    {
        return new FilterProperty<TProperty>
        {
            HasValue = false
        };
    }
}

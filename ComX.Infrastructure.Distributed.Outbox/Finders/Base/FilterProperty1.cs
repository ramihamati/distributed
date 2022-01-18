namespace ComX.Infrastructure.Distributed.Outbox;

public static class FilterProperty
{
    public static FilterProperty<T> From<T>(T value)
    {
        if (value is string strValue)
        {
            return !string.IsNullOrWhiteSpace(strValue)
                ? FilterProperty<T>.SetValue(value)
                : FilterProperty<T>.SetNoValue();
        }

        return value is null
            ? FilterProperty<T>.SetNoValue()
            : FilterProperty<T>.SetValue(value);
    }

    public static FilterProperty<T> Copy<T>(FilterProperty<T> value) 
    {
        return value is null || !value.HasValue
            ? FilterProperty<T>.SetNoValue()
            : FilterProperty<T>.SetValue(value.Value);
    }

    public static FilterProperty<T> From<T>(T? value) where T : struct
    {
        return value is null || !value.HasValue
            ? FilterProperty<T>.SetNoValue()
            : FilterProperty<T>.SetValue(value.Value);
    }
}

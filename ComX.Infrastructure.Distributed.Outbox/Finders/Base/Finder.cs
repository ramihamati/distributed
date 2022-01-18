namespace ComX.Infrastructure.Distributed.Outbox;

public class Finder<TFilter> where TFilter : new()
{
    public int? Limit { get; set; } = null;

    public int? Skip { get; set; } = null;

    public TFilter Filter { get; set; } = default;

    public Finder()
    {
        Filter = new TFilter();
    }
}

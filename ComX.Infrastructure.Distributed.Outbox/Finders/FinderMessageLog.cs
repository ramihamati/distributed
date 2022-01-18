namespace ComX.Infrastructure.Distributed.Outbox
{
    public class FinderMessageLog : Finder<FilterMessageLog>
    {
        public static FinderMessageLog Empty
        {
            get
            {
                return new FinderMessageLog()
                {
                    Filter = FilterMessageLog.Empty,
                };
            }
        }

        public static FinderMessageLog New(FilterMessageLog filter, int? limit = null, int? skip = null)
        {
            return new FinderMessageLog
            {
                Limit = limit,
                Skip = skip,
                Filter = filter
            };
        }
    }
}

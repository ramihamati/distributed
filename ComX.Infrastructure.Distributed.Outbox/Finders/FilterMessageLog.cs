namespace ComX.Infrastructure.Distributed.Outbox
{
    public class FilterMessageLog
    {
        public FilterProperty<OutboxStatus> Status { get; private set; } = FilterProperty<OutboxStatus>.SetNoValue();
        public FilterProperty<TimeSpan> LastAttemptOffset { get; private set; } = FilterProperty<TimeSpan>.SetNoValue();
        public FilterProperty<bool> Unlocked { get; private set; } = FilterProperty<bool>.SetNoValue();
        public FilterProperty<string> MessageTypeName { get; private set; } = FilterProperty<string>.SetNoValue();

        public static FilterMessageLog Empty
        {
            get
            {
                return new FilterMessageLog();
            }
        }

        public FilterMessageLog SetStatus(OutboxStatus status)
        {
            this.Status = FilterProperty<OutboxStatus>.SetValue(status);
            return this;
        }

        public FilterMessageLog SetLastAttemptOffset(TimeSpan lastAttemptOffset)
        {
            this.LastAttemptOffset = FilterProperty<TimeSpan>.SetValue(lastAttemptOffset);
            return this;
        }

        public FilterMessageLog SetUnlocked(bool unlocked)
        {
            this.Unlocked = FilterProperty<bool>.SetValue(unlocked);
            return this;
        }

        public FilterMessageLog SetMessageTypeName(string messageTypeName)
        {
            this.MessageTypeName = FilterProperty<string>.SetValue(messageTypeName);
            return this;
        }
    }
}

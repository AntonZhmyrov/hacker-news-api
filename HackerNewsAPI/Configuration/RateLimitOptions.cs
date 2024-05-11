namespace HackerNewsAPI.Configuration
{
    public class RateLimitOptions
    {
        public const string Section = "RateLimit";
        public const string PolicyName = "fixed";

        public int PermitLimit { get; set; }

        public int TimeWindowSeconds { get; set; }

        public int QueueLimit { get; set; }
    }
}

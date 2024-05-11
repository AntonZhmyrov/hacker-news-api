namespace HackerNewsAPI.Helpers
{
    public static class DateTimeHelper
    {
        public static string FromUnixSeconds(long unixTimeSeconds)
            => DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).ToLocalTime().ToString("yyyy-MM-ddTHH-mm-ssK");
    }
}

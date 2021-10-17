namespace EventManager
{
    public static class AppVersion
    {
        public static string Get()
        {
            var versionString = typeof(AppVersion).Assembly.GetName().Version?.ToString() ?? "unknown";

            return versionString;
        }
    }
}

namespace dotnet_unpkg
{
    public static class TrimExtensions
    {
        public static string TrimSlashes(this string path) => path?.Trim().Trim('/');
    }
}
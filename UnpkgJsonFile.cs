namespace dotnet_unpkg
{
    public class UnpkgJsonFile
    {
        public string Path { get; set; }
        public string Integrity { get; set; }
        public string CdnUrl { get; set; }
        public string LocalPath { get; set; }
    }
}
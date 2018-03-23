using System;

namespace dotnet_unpkg
{
    public class DistEntry
    {
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Integrity { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace dotnet_unpkg
{
    public class DistFile
    {
        public string BaseUrl { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string ContentType { get; set; }
        public string Path { get; set; }
        public string LocalPath { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Integrity { get; set; }
        public List<DistFile> Files { get; set; }
    }
}
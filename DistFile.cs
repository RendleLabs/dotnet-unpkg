using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace dotnet_unpkg
{
    [PublicAPI]
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
        public string Url { get; set; }
        public string Version { get; set; }
    }
}
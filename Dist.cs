using System.Collections.Generic;
using System.Net.Http;

namespace dotnet_unpkg
{
    public class Dist
    {
        private static readonly HttpClient Client = new HttpClient();
        
        public static IEnumerable<DistEntry> Get(string package)
        {
            
        }
    }
}
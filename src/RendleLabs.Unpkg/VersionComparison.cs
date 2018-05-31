using System;
using Semver;

namespace RendleLabs.Unpkg
{
    public static class VersionComparison
    {
        public static bool IsGreater(string left, string right)
        {
            if (left.Equals(right, StringComparison.OrdinalIgnoreCase)) return false;
            
            if (SemVersion.TryParse(left, out var lv))
            {
                if (SemVersion.TryParse(right, out var rv))
                {
                    return lv > rv;
                }
                else
                {
                    Console.WriteLine($"'{right}' makes no sense as a version number.");
                }
            }
            else
            {
                Console.WriteLine($"'{left}' makes no sense as a version number.");
            }

            return false; // Don't know, let's be pessimistic
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RendleLabs.Unpkg.Build
{
    public class RestoreTask : Task
    {
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string ProjectDirectory { get; set; }
        
        [Output]
        public ITaskItem[] FilesWritten { get; set; }
        
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, $"{Environment.NewLine}Restoring UNPKG libraries");

            var results = Restore.Run().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(results.Error))
            {
                Log.LogError(results.Error);
                return false;
            }

            int errorCount = results.Results.Count(r => !string.IsNullOrWhiteSpace(r.Error));
            if (errorCount > 0)
            {
                Log.LogWarning($"{Environment.NewLine}UNPKG restore completed with {errorCount} errors.");
            }
            else
            {
                Log.LogMessage(MessageImportance.High, $"{Environment.NewLine}UNPKG restore completed.");
            }

            var pattern = $"^{Regex.Escape(ProjectDirectory)}[/\\\\]";
            var regex = new Regex(pattern);

            FilesWritten = results.Results
                .Where(r => r.LocalFile != null)
                .Select(r => regex.Replace(r.LocalFile, ""))
                .Select(s => new TaskItem(s))
                .ToArray();

            return true;
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EventCore
{
    public class ModFile
    {
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public Dictionary<string, FTLEvent> Events { get; }
        public ModFile(string filePath)
        {
            FilePath = filePath;
            Events = new Dictionary<string, FTLEvent>();

        }
    }
}

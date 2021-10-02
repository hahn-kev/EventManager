using System.Collections.Generic;
using System.Linq;

namespace EventCore
{
    public class ModFile
    {
        public string FilePath { get; set; }
        public Dictionary<string, FTLEvent> Events { get; }
        public ModFile(string filePath, FTLEvent[] events)
        {
            FilePath = filePath;
            Events = new Dictionary<string, FTLEvent>(events.Length);
            foreach (var ftlEvent in events)
            {
                if (ftlEvent.Name == null) continue;
                Events[ftlEvent.Name] = ftlEvent;
            }
        }
    }
}

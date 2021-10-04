using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Xml.Dom;

namespace EventCore
{
    public class ModFile
    {
        public ModRoot? ModRoot { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public Dictionary<string, FTLEvent> Events { get; }

        public IXmlDocument? Document { get; set; }

        public ModFile(string filePath)
        {
            FilePath = filePath;
            Events = new Dictionary<string, FTLEvent>();
        }
    }
}

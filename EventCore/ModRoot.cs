using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EventCore
{
    public class ModRoot
    {
        public ModRoot(string folderPath, ModFile[] modFiles)
        {
            FolderPath = folderPath;
            ModFiles = modFiles.ToDictionary(f => Path.GetFileName(f.FilePath));
            foreach (var modFile in modFiles)
            {
                modFile.ModRoot = this;
            }
        }

        public string FolderPath { get; set; }

        public Dictionary<string, ModFile> ModFiles { get; }
        public IEnumerable<FTLEvent> TopLevelEvents => ModFiles.Values.SelectMany(mf => mf.Events.Values);

        public Dictionary<string, FTLEvent> EventsLookup =>
            ModFiles.Values.SelectMany(mf => mf.Events)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(g => g.Key, g => g.Last());
    }
}

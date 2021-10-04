using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EventCore
{
    public class ModLoader
    {
        private readonly string _folderPath;
        private Dictionary<string, FTLEvent> Events = new();
        private List<FTLEventRef> EventRefs = new();

        public ModLoader(string folderPath)
        {
            _folderPath = folderPath;
        }

        public async Task<ModRoot> Load()
        {
            Events = new Dictionary<string, FTLEvent>();
            EventRefs = new List<FTLEventRef>();
            var modFileLoaders = Directory.EnumerateFiles(_folderPath, "*.xml*")
                .Select(filePath => new ModFileLoader(filePath)).ToArray();

            // Parallel.ForEach(modFileLoaders,loader => loader.Load());
            foreach (var modFileLoader in modFileLoaders)
            {
                modFileLoader.Load();
            }
            // await Task.WhenAll(modFileLoaders.Select(mfl => mfl.Load()));

            EventRefs.AddRange(modFileLoaders.SelectMany(loader => loader.EventRefs));
            Events = modFileLoaders.SelectMany(d => d.Events)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(g => g.Key, g => g.Last());

            LinkMissingEvents();
            return new ModRoot(_folderPath, modFileLoaders.Select(loader => loader.ModFile!).ToArray());
        }

        private void LinkMissingEvents()
        {
            foreach (var ftlEventRef in EventRefs)
            {
                if (ftlEventRef.IsUnknownRef)
                {
                    ftlEventRef.FindRef(Events);
                }
            }
        }
    }
}

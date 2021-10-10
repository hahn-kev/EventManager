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
        private List<FTLEventRef> EventRefs = new();

        public ModLoader(string folderPath)
        {
            _folderPath = folderPath;
        }

        public async Task<ModRoot> Load()
        {
            EventRefs = new List<FTLEventRef>();


            ModFileLoader[] modFileLoaders = ListEventFiles(out var hyperspaceLoader)
                .Select(fileName => Path.Combine(_folderPath, fileName))
                .Intersect(Directory.EnumerateFiles(_folderPath, "*.xml*"))
                .Select(filePath => new ModFileLoader(filePath))
                .Append(hyperspaceLoader)
                .Where(loader => loader != null)
                .ToArray()!;

            // Parallel.ForEach(modFileLoaders,loader => loader.Load());
            foreach (var modFileLoader in modFileLoaders)
            {
                modFileLoader.Load();
            }

            // await Task.WhenAll(modFileLoaders.Select(mfl => mfl.Load()));
            modFileLoaders = modFileLoaders.Where(file => file.Events.Count > 0).ToArray();

            EventRefs.AddRange(modFileLoaders.SelectMany(loader => loader.EventRefs));

            var modRoot = new ModRoot(_folderPath,
                modFileLoaders.Select(loader => loader.ModFile).ToArray());

            LinkEventRefs(modRoot.EventsLookup);
            return modRoot;
        }

        private IEnumerable<string> ListEventFiles(out ModFileLoader? hyperspaceLoader)
        {
            var hyperspacePath = Path.Combine(_folderPath, "hyperspace.xml");
            if (!File.Exists(hyperspacePath))
            {
                hyperspaceLoader = null;
                return new[]
                {
                    "events.xml",
                    "events.xml.append"
                };
            }

            hyperspaceLoader = new ModFileLoader(hyperspacePath);
            hyperspaceLoader.Load();
            var hyperspaceDocument = hyperspaceLoader.ModFile.Document;
            var eventFiles = hyperspaceDocument.QuerySelectorAll("eventFile").ToArray();
            return eventFiles.Select(e => $"events_{e.TextContent}.xml");
        }

        private void LinkEventRefs(Dictionary<string, FTLEvent> ftlEvents)
        {
            foreach (var ftlEventRef in EventRefs)
            {
                if (ftlEventRef.IsUnknownRef)
                {
                    ftlEventRef.FindRef(ftlEvents);
                }
            }
        }
    }
}

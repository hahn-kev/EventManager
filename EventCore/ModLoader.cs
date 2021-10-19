using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using EventCore.FTL;

namespace EventCore
{
    public class ModLoader
    {
        private readonly string _folderPath;
        private List<FTLEventRef> EventRefs = new();

        private static readonly string[] DefaultEventFiles = new[]
        {
            "events.xml",
            "events_boss.xml",
            "events_crystal.xml",
            "events_engi.xml",
            "events_fuel.xml",
            "events_imageList.xml",
            "events_mantis.xml",
            "events_nebula.xml",
            "events_pirate.xml",
            "events_rebel.xml",
            "events_rock.xml",
            "events_ships.xml",
            "events_slug.xml",
            "events_zoltan.xml",
            "dlcEvents.xml",
            "dlcEventsOverwrite.xml",
            "dlcEvents_anaerobic.xml",
            "text_events.xml"
        };

        public ModLoader(string folderPath)
        {
            _folderPath = folderPath;
        }

        public ModRoot Load()
        {
            EventRefs = new List<FTLEventRef>();


            ModFileLoader[] modFileLoaders = ListLoaders().ToArray()!;

            // Parallel.ForEach(modFileLoaders,loader => loader.Load());
            foreach (var modFileLoader in modFileLoaders)
            {
                modFileLoader.Load();
            }

            // await Task.WhenAll(modFileLoaders.Select(mfl => mfl.Load()));
            modFileLoaders = modFileLoaders.Where(file => file.Events.Count > 0 || file.TextRefs.Count > 0).ToArray();

            EventRefs.AddRange(modFileLoaders.SelectMany(loader => loader.EventRefs));

            var modRoot = new ModRoot(_folderPath,
                modFileLoaders.Select(loader => loader.ModFile).ToArray());

            LinkEventRefs(modRoot);
            return modRoot;
        }

        private IEnumerable<ModFileLoader> ListLoaders()
        {
            return ListEventFiles(out var hyperspaceLoader)
                .Select(fileName => Path.Combine(_folderPath, fileName))
                .Intersect(Directory.EnumerateFiles(_folderPath, "*.xml*"))
                .Select(filePath => new ModFileLoader(filePath))
                .Append(hyperspaceLoader)
                .Where(loader => loader != null)!;
        }

        private IEnumerable<string> ListEventFiles(out ModFileLoader? hyperspaceLoader)
        {
            var hyperspacePath = Path.Combine(_folderPath, "hyperspace.xml");
            var allDefaultEventFiles = DefaultEventFiles.Concat(DefaultEventFiles.Select(fileName => fileName + ".append"));
            if (!File.Exists(hyperspacePath))
            {
                hyperspaceLoader = null;
                return allDefaultEventFiles;
            }

            hyperspaceLoader = new ModFileLoader(hyperspacePath);
            hyperspaceLoader.Load();
            var hyperspaceDocument = hyperspaceLoader.ModFile.Document;
            var eventFiles = hyperspaceDocument.QuerySelectorAll("eventFile").ToArray();
            return eventFiles.Select(e => $"events_{e.TextContent}.xml").Concat(allDefaultEventFiles);
        }

        private void LinkEventRefs(ModRoot modRoot)
        {
            foreach (var ftlEventRef in EventRefs)
            {
                if (ftlEventRef.IsUnknownRef)
                {
                    ftlEventRef.FindRef(modRoot.EventsLookup);
                }
            }

            foreach (var ftlEvent in modRoot.ModFiles.Values.SelectMany(mf => mf.AllEvents))
            {
                ftlEvent.FindTextRef();
            }
        }
    }
}

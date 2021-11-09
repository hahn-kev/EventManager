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
            "nameEvents.xml",
            "newEvents.xml",
            "dlcEvents.xml",
            "dlcEventsOverwrite.xml",
            "dlcEvents_anaerobic.xml",
            "text_events.xml"
        };

        public ModLoader(string folderPath)
        {
            if (Directory.Exists(Path.Combine(folderPath, "mod-appendix")))
            {
                folderPath = Path.Combine(folderPath, "data");
            }
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
            var allDefaultEventFiles = IncludeAppendFiles(DefaultEventFiles);
            var hyperspacePath = GetValidHyperspacePath();
            if (hyperspacePath is null)
            {
                hyperspaceLoader = null;
                return allDefaultEventFiles;
            }

            hyperspaceLoader = new ModFileLoader(hyperspacePath);
            hyperspaceLoader.Load();
            var hyperspaceDocument = hyperspaceLoader.ModFile.Document;
            var eventFiles = hyperspaceDocument.QuerySelectorAll("eventFile");
            return IncludeAppendFiles(eventFiles.Select(e => $"events_{e.TextContent}.xml").ToArray())
                .Concat(allDefaultEventFiles);
        }

        private static IEnumerable<string> IncludeAppendFiles(string[] eventFiles)
        {
            return eventFiles.Concat(eventFiles.Select(fileName => fileName + ".append"));
        }

        private string? GetValidHyperspacePath()
        {
            var hyperspacePath = Path.Combine(_folderPath, "hyperspace.xml");
            if (File.Exists(hyperspacePath)) return hyperspacePath;
            hyperspacePath = Path.ChangeExtension(hyperspacePath, ".xml.append");
            return File.Exists(hyperspacePath) ? hyperspacePath : null;
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

            foreach (var ftlEvent in modRoot.ModFiles.Values.SelectMany(mf => mf.AllCanRefTexts))
            {
                ftlEvent.FindTextRef();
            }
        }
    }
}

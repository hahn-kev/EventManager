using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Xml.Parser;

namespace EventCore
{
    public class ModFileLoader
    {
        private string _filePath;
        public Dictionary<string, FTLEvent> Events => ModFile.Events;
        public List<FTLEventRef> EventRefs { get; } = new();
        public ModFile ModFile { get; set; }

        private bool _loaded;

        public ModFileLoader(string filePath)
        {
            _filePath = filePath;
            ModFile = new ModFile(_filePath);
        }

        public void Load()
        {
            if (_loaded) return;
            _loaded = true;

            var xmlParser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var fileText =  File.ReadAllText(_filePath);

            var document = xmlParser.ParseDocument(fileText);

            ModFile.Document = document;
            var ftlEvents = ParseEvents(document.Children);
            foreach (var ftlEvent in ftlEvents)
            {
                if (ftlEvent.Name == null) continue;
                ModFile.Events[ftlEvent.Name] = ftlEvent;
            }
        }

        private static readonly string[] eventParentTags = new[] { "FTL", "events" };
        private IEnumerable<FTLEvent> ParseEvents(IEnumerable<IElement> elements)
        {
            foreach (var xElement in elements)
            {
                if (eventParentTags.Contains(xElement.TagName, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (var @event in ParseEvents(xElement.Children))
                    {
                        yield return @event;
                    }

                    yield break;
                }

                if (xElement.TagName != "event") continue;
                var ftlEvent = EventElementToModel(xElement);
                yield return ftlEvent;
            }
        }

        private FTLEvent EventElementToModel(IElement element)
        {
            var ftlChoices = FTLEvent.IsEventRef(element, out _)
                ? new List<FTLChoice>()
                : element.Children.Where(e => e.TagName == "choice").Select(ChoiceElementToModel)
                .ToList();
            var ftlEvent = FTLEvent.NewEvent(element, ModFile, ftlChoices);

            if (ftlEvent.Name is not null) Events[ftlEvent.Name] = ftlEvent;
            if (ftlEvent is FTLEventRef @ref) EventRefs.Add(@ref);
            return ftlEvent;
        }

        private FTLChoice ChoiceElementToModel(IElement element, int index)
        {
            var eventElement = element.Element("event");

            if (eventElement is null)
            {
                throw new NotSupportedException();
            }

            var ftlEvent = EventElementToModel(eventElement);
            return new FTLChoice(index, ftlEvent, element, ModFile);
        }
    }
}

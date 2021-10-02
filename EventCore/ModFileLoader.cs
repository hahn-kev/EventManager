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
        public Dictionary<string, FTLEvent> Events { get; } = new Dictionary<string, FTLEvent>();
        public List<FTLEventRef> EventRefs { get; } = new List<FTLEventRef>();
        public ModFile? ModFile { get; set; }

        public ModFileLoader(string filePath)
        {
            _filePath = filePath;
        }

        public async Task Load()
        {
            var xmlParser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            await using var fileStream = File.OpenRead(_filePath);

            var document = await xmlParser.ParseDocumentAsync(fileStream);

            // using var xmlReader = XmlReader.Create(File.OpenRead(_filePath),
            //     new XmlReaderSettings
            //     {
            //         CloseInput = true,
            //         IgnoreComments = true,
            //         ConformanceLevel = ConformanceLevel.Fragment,
            //     });

            var ftlEvents = ParseEvents(document.Children).ToArray();
            ModFile = new ModFile(_filePath, ftlEvents);
        }

        private IEnumerable<FTLEvent> ParseEvents(IEnumerable<IElement> elements)
        {
            foreach (var xElement in elements)
            {
                if (xElement.TagName == "FTL")
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
            FTLEvent Inner()
            {
                if (IsEventRef(element, out var name)) return FTLEvent.EventRef(element, name);
                // if (element) return FTLEvent.Nothing;

                var xAttribute = element.GetAttribute("name");
                var textElement = element.Element("text");
                var ftlChoices = element.Children.Where(e => e.TagName == "choice").Select(ChoiceElementToModel)
                    .ToList();
                return new FTLEvent(element, xAttribute, textElement?.TextContent, ftlChoices);
            }

            var ftlEvent = Inner();

            if (ftlEvent.Name is not null && ftlEvent is not FTLEventRef) Events[ftlEvent.Name] = ftlEvent;
            if (ftlEvent is FTLEventRef @ref) EventRefs.Add(@ref);
            return ftlEvent;
        }

        private bool IsEventRef(IElement element, [NotNullWhen(true)] out string? name)
        {
            var loadAttr = element.GetAttribute("load");
            if (loadAttr is not null)
            {
                name = loadAttr;
                return true;
            }

            var loadEventElement = element.Element("loadEvent");
            if (loadEventElement is not null)
            {
                name = loadEventElement.TextContent;
                return true;
            }

            name = null;
            return false;
        }

        private FTLChoice ChoiceElementToModel(IElement element)
        {
            var textElement = element.Element("text") ?? throw new NotSupportedException("choice must have text");
            var eventElement = element.Element("event");

            if (eventElement is null)
            {
                throw new NotSupportedException();
            }

            var ftlEvent = EventElementToModel(eventElement);
            return new FTLChoice(element.GetAttribute("hidden") == "true",
                textElement.TextContent,
                ftlEvent,
                element.GetAttribute("req"),
                element);
        }
    }
}

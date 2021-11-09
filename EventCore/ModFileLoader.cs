using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Xml.Dom;
using AngleSharp.Xml.Parser;
using EventCore.FTL;

namespace EventCore
{
    public class ModFileLoader
    {
        private string _filePath;
        public Dictionary<string, FTLEvent> Events => ModFile.Events;
        public List<FTLEventRef> EventRefs { get; } = new();
        public List<ICanHaveTextRef> AllCanRefTexts => ModFile.AllCanRefTexts;
        public Dictionary<string, FTLTextRef> TextRefs => ModFile.TextRefs;
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


            var document = ParseDocument(_filePath);
            ModFile.Document = document;
            ParseElements(document.Children);
        }

        private IXmlDocument ParseDocument(string filePath)
        {
            var xmlParser = new XmlParser(new XmlParserOptions
            {
                IsSuppressingErrors = true
            });
            var fileText = File.ReadAllText(filePath);

            var document = xmlParser.ParseDocument(fileText);
            if (document.DocumentElement?.TagName == "FTL") return document;
            document = xmlParser.ParseDocument($"<FTL>\r\n{fileText}\r\n</FTL>");
            return document;
        }

        private static readonly string[] eventParentTags = new[] { "FTL", "events" };

        private void ParseElements(IEnumerable<IElement> elements)
        {
            foreach (var xElement in elements)
            {
                ParseElement(xElement);
            }
        }

        private void ParseElement(IElement xElement)
        {
            if (eventParentTags.Contains(xElement.TagName, StringComparer.OrdinalIgnoreCase))
            {
                ParseElements(xElement.Children);
                return;
            }

            switch (xElement.TagName)
            {
                case "event":
                    ParseEvent(xElement);
                    break;
                case "text":
                    ParseText(xElement);
                    break;
                case "eventList":
                    ParseEventList(xElement);
                    break;
            }
        }

        private void ParseText(IElement element)
        {
            var textRef = new FTLTextRef(element);
            TextRefs[textRef.Name] = textRef;
        }

        private void ParseEventList(IElement xElement)
        {
            if (!xElement.HasAttribute("name"))
                return;
            var events = xElement.Children.Where(e => e.TagName == "event").Select(EventElementToModel).ToList();

            var eventList = new FTLEventList(xElement, events, ModFile);
            var eventListName = eventList.Name;
            if (eventListName == null)
                throw new Exception("no name found for event list");

            ModFile.Events[eventListName] = eventList;
        }

        private void ParseEvent(IElement xElement)
        {
            EventElementToModel(xElement);
        }

        private FTLEvent EventElementToModel(IElement element)
        {
            var ftlChoices = FTLEvent.IsEventRef(element, out _)
                ? new List<FTLChoice>()
                : element.Children.Where(e => e.TagName == "choice").Select(ChoiceElementToModel)
                    .ToList();
            var ftlEvent = FTLEvent.ParseEvent(element, ModFile, ftlChoices);
            var name = element.GetAttribute("name");
            if (name is not null) Events[name] = ftlEvent;
            if (ftlEvent is FTLEventRef @ref) EventRefs.Add(@ref);
            else AllCanRefTexts.Add(ftlEvent);
            return ftlEvent;
        }

        private FTLChoice ChoiceElementToModel(IElement element, int index)
        {
            var eventElement = element.Element("event");

            if (eventElement is null)
            {
                throw new NotSupportedException("choice must have an event");
            }

            var ftlEvent = EventElementToModel(eventElement);
            var choice = new FTLChoice(index, ftlEvent, element, ModFile);
            AllCanRefTexts.Add(choice);
            return choice;
        }
    }
}

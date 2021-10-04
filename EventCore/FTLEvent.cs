using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace EventCore
{
    public class FTLEvent
    {
        public static readonly FTLEvent Nothing = new FTLEvent();

        private FTLEvent()
        {
        }

        protected FTLEvent(IElement xElement, ModFile modFile)
        {
            ModFile = modFile;
            Element = xElement;
        }

        public FTLEvent(IElement xElement, string? name, List<FTLChoice> choices, ModFile modFile) : this(xElement,
            modFile)
        {
            Name = name;
            Choices = choices;
            ModFile = modFile;
        }

        public IElement Element { get; init; }
        public ModFile ModFile { get; set; }
        private string? _name;

        public virtual string? Name
        {
            get => _name;
            set
            {
                _name = value;
                if (value != null)
                    Element.SetAttribute("name", value);
                else
                    Element.RemoveAttribute("name");
            }
        }

        public string? Text
        {
            get => Element.Element("text")?.TextContent;
            set
            {
                var element = Element.Element("text");
                if (element != null)
                {
                    element.TextContent = value ?? "";
                }
                else
                {
                    var htmlElement = Element.Owner!.CreateElement("text");
                    htmlElement.TextContent = value ?? "";
                    Element.AppendChild(htmlElement);
                }
            }
        }

        public bool HasReward => Element.Element("autoReward") != null;
        public string? RewardLevel => Element.Element("autoReward")?.GetAttribute("level");
        public string? RewardType => Element.Element("autoReward")?.TextContent;

        public bool HasCrew => Element.Element("crewMember") != null;
        public int CrewAmount => int.Parse(Element.Element("crewMember")?.GetAttribute("amount") ?? "0");
        public string? CrewClass => Element.Element("crewMember")?.GetAttribute("class");

        public virtual List<FTLChoice> Choices { get; } = new();
        public virtual bool IsUnknownRef => false;
    }

    public class FTLEventRef : FTLEvent
    {
        private FTLEvent? ActualEvent;
        private string _refName;

        public FTLEventRef(IElement xElement, string refName, ModFile modFile) : base(xElement, modFile)
        {
            Element = xElement;
            _refName = refName;
        }

        public override List<FTLChoice> Choices => ActualEvent!.Choices;

        public override string? Name
        {
            get => _refName;
            set
            {
                _refName = value!;
                if (Element.HasAttribute("load"))
                {
                    Element.SetAttribute("load", _refName);
                }
                else
                {
                    var loadEventElement = Element.Element("loadEvent");
                    loadEventElement.TextContent = _refName;
                }

                FindRef(ModFile.ModRoot?.EventsLookup ?? ModFile.Events);
            }
        }

        public override bool IsUnknownRef => ActualEvent == null;

        public void FindRef(Dictionary<string, FTLEvent> events)
        {
            events.TryGetValue(_refName, out ActualEvent);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AngleSharp.Dom;

namespace EventCore
{
    public class FTLEvent
    {
        public static readonly FTLEvent Nothing = new FTLEvent();

        public static FTLEvent EventRef(IElement xElement, ModFile modFile, string name)
        {
            return new FTLEventRef(xElement, name, modFile);
        }

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

        public string? Name
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

        public string Text
        {
            get => Element.Element("text").TextContent;
            set => Element.Element("text").TextContent = value;
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
        private readonly string _refName;

        public FTLEventRef(IElement xElement, string refName, ModFile modFile) : base(xElement, modFile)
        {
            Element = xElement;
            _refName = refName;
            Name = refName;
        }

        public override List<FTLChoice> Choices => ActualEvent!.Choices;

        public override bool IsUnknownRef => ActualEvent == null;

        public void FindRef(Dictionary<string, FTLEvent> events)
        {
            events.TryGetValue(_refName, out ActualEvent);
        }
    }
}

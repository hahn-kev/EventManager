using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AngleSharp.Dom;

namespace EventCore
{
    public class FTLEvent
    {
        public static readonly FTLEvent UnknownEventRef = new FTLEvent();
        public static readonly FTLEvent Nothing = new FTLEvent();

        public static FTLEvent EventRef(IElement xElement, string name)
        {
            return new FTLEventRef(xElement, name);
        }

        protected FTLEvent()
        {
        }

        public FTLEvent(IElement xElement, string? name, string? text, List<FTLChoice> choices)
        {
            Element = xElement;
            Name = name;
            Text = text;
            Choices = choices;
        }

        public IElement Element { get; init; }
        public string? Name { get; set; }
        public virtual string? Text { get; set; }
        public bool HasReward => Element.Element("autoReward") != null;
        public string? RewardLevel => Element.Element("autoReward")?.GetAttribute("level");
        public string? RewardType => Element.Element("autoReward")?.TextContent;

        public bool HasCrew => Element.Element("crewMember") != null;
        public int CrewAmount => int.Parse(Element.Element("crewMember")?.GetAttribute("amount") ?? "0");
        public string? CrewClass => Element.Element("crewMember")?.GetAttribute("class");

        public virtual List<FTLChoice> Choices { get; }
        public virtual bool IsUnknownRef => false;
    }

    public class FTLEventRef : FTLEvent
    {
        private FTLEvent? ActualEvent;
        private readonly string _refName;

        public FTLEventRef(IElement xElement, string refName)
        {
            Element = xElement;
            _refName = refName;
            Name = refName;
        }

        public override string? Text
        {
            get => ActualEvent!.Text;
            set => throw new NotSupportedException();
        }

        public override List<FTLChoice> Choices => ActualEvent!.Choices;

        public override bool IsUnknownRef => ActualEvent == null;

        public void FindRef(Dictionary<string, FTLEvent> events)
        {
            events.TryGetValue(_refName, out ActualEvent);
        }
    }
}

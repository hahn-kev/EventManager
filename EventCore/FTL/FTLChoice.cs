using System;
using System.Collections.Generic;
using AngleSharp.Dom;

namespace EventCore.FTL
{
    public class FTLChoice: ICanHaveTextRef
    {
        private static readonly string[] ValidAttributes = new[] {
            "hidden", "req", "lvl", "blue", "max_group", "max_lvl", "min_level"
        };
        private static readonly string[] ValidChildElements = new[] {
            "text", "event", "store", "choice"
        };

        public FTLChoice(int index, FTLEvent @event, IElement element, ModFile modFile)
        {
            Index = index + 1;
            Event = @event;
            Element = element;
            ModFile = modFile;
        }

        public IElement Element { get; }
        public ModFile ModFile { get; }

        public bool Hidden
        {
            get => Element.GetAttribute("hidden") == "true";
            set
            {
                if (value)
                    Element.SetAttribute("hidden", "true");
                else
                    Element.RemoveAttribute("hidden");
            }
        }

        public int Index { get; }
        public string? Text
        {
            get => ((ICanHaveTextRef)this).TextImp;
            set => ((ICanHaveTextRef)this).TextImp = value;
        }

        FTLTextRef? ICanHaveTextRef.TextRef { get; set; }

        public FTLEvent Event { get; set; }
        public string? Requirement
        {
            get => Element.GetAttribute("req");
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Element.RemoveAttribute("req");
                    Element.RemoveAttribute("lvl");
                }
                else
                {
                    Element.SetAttribute("req", value);
                }
            }
        }

        public int RequirementLevel
        {
            get => int.Parse(Element.GetAttribute("lvl") ?? "0");
            set => Element.SetAttribute("lvl", value.ToString());
        }

        public void ConvertToInlineEvent()
        {
            var eventElement = Event.Element;
            eventElement.RemoveAttribute("load");
            eventElement.RemoveChildElement("loadEvent");
            Event = FTLEvent.ParseEvent(eventElement, ModFile, new List<FTLChoice>());
        }

        public void ConvertToLoadEvent()
        {
            if (Event.Element.Children.Length > 0)
            {
                throw new NotSupportedException("to convert to a load event there must be no children of the element");
            }

            var eventElement = Event.Element;
            eventElement.AppendNew("loadEvent", selfClosing:false);

            Event = FTLEvent.ParseEvent(eventElement, ModFile, new List<FTLChoice>());
        }
    }
}

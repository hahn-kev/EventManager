using System;
using AngleSharp.Dom;

namespace EventCore
{
    public class FTLChoice
    {
        private static readonly string[] ValidAttributes = new[] {
            "hidden", "req", "lvl", "blue", "max_group", "max_lvl", "min_level"
        };
        private static readonly string[] ValidChildElements = new[] {
            "text", "event", "store", "choice"
        };

        private IElement _textElement;

        public FTLChoice(int index, FTLEvent @event, IElement element)
        {
            Index = index + 1;
            _textElement = element.Element("text") ?? throw new NotSupportedException("choice must have text");

            Event = @event;
            Element = element;
        }

        public IElement Element { get; }

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
        public string Text
        {
            get => _textElement.TextContent;
            set => _textElement.TextContent = value;
        }

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
    }
}

using AngleSharp.Dom;

namespace EventCore
{
    public class FTLChoice
    {
        public FTLChoice(bool hidden, string text, FTLEvent @event, string? requirement, IElement element)
        {
            Hidden = hidden;
            Text = text;
            Event = @event;
            Requirement = requirement;
            Element = element;
        }

        public IElement Element { get; }

        public bool Hidden { get; set; }
        public string Text { get; set; }
        public FTLEvent Event { get; set; }
        public string? Requirement { get; set; }
    }
}

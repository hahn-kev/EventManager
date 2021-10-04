using AngleSharp.Dom;

namespace EventCore
{
    public class FTLChoice
    {
        public FTLChoice(bool hidden, int index, string text, FTLEvent @event, string? requirement, IElement element)
        {
            Hidden = hidden;
            Index = index + 1;
            Text = text;
            Event = @event;
            Requirement = requirement;
            Element = element;
        }

        public IElement Element { get; }

        public bool Hidden { get; set; }
        public int Index { get; }
        public string Text { get; set; }
        public FTLEvent Event { get; set; }
        public string? Requirement { get; set; }
    }
}

using AngleSharp.Dom;

namespace EventCore
{
    public class FTLTextRef
    {
        private readonly IElement _element;

        public FTLTextRef(IElement element)
        {
            _element = element;
        }

        public string Name
        {
            get => _element.GetAttribute("name") ?? "";
            set => _element.SetAttribute("name", value);
        }

        public string Text
        {
            get => _element.TextContent;
            set => _element.TextContent = value;
        }
    }
}

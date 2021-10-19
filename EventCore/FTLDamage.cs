using AngleSharp.Dom;

namespace EventCore
{
    public class FTLDamage
    {
        public IElement Element { get; }

        public FTLDamage(IElement element)
        {
            Element = element;
        }

        public int Amount
        {
            get => int.Parse(Element.GetAttribute("amount") ?? "0");
            set => Element.SetAttribute("amount", value.ToString());
        }

        public string System
        {
            get => Element.GetAttribute("system") ?? "";
            set => Element.SetAttributeRemoveIfBlank("system", value);
        }

        public string Effect
        {
            get => Element.GetAttribute("effect") ?? "";
            set => Element.SetAttributeRemoveIfBlank("effect", value);
        }
    }
}

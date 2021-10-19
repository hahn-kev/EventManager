using System;
using AngleSharp.Dom;

namespace EventCore.FTL
{
    public interface ICanHaveTextRef
    {
        public ModFile ModFile { get; }
        public IElement Element { get; }
        public string? Text { get; set; }
        protected internal string? TextImp
        {
            get
            {
                if (IsUnknownTextRef) return $"[Unknown id='{TextRefId}' ]";
                return TextRef?.Text ?? Element.Element("text")?.TextContent;
            }
            set
            {
                if (TextRef != null)
                {
                    TextRef.Text = value ?? "";
                    return;
                }

                Element.SetChildElementText("text", value ?? "", true, true);

            }
        }
        public FTLTextRef? TextRef { get; protected set; }
        public bool IsTextRef => Element.Element("text")?.HasAttribute("id") ?? false;
        public string? TextRefId => Element.Element("text")?.GetAttribute("id");
        public bool IsUnknownTextRef => IsTextRef && TextRef == null;

        public void FindTextRef()
        {
            var textRefId = TextRefId;
            if (!IsTextRef) return;
            if (textRefId == null) throw new Exception("text ref id unknown");
            var textRefs = ModFile.ModRoot?.TextRefs ?? ModFile.TextRefs;
            if (!textRefs.TryGetValue(textRefId, out var ftlTextRef))
            {
                // throw new Exception($"unable to find text id '{textRefId}'");
                return;
            }

            TextRef = ftlTextRef;
        }
    }
}

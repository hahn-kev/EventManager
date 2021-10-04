using System.Security;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Xml;

namespace EventCore
{
    public class FtlXmlMarkupFormatter : XmlMarkupFormatter
    {
        public new static readonly IMarkupFormatter Instance = new FtlXmlMarkupFormatter();
        public override string Text(ICharacterData text)
        {
            return XmlMarkupFormatter.EscapeText(NormalizeLineEndings(text.Data));
            // if (text.Data == "\n") return "\r\n";
            // return base.Text(text);
        }

        public override string Comment(IComment comment)
        {
            return "<!--" + NormalizeLineEndings(comment.Data) + "-->";
        }

        private string NormalizeLineEndings(string s)
        {
            return s.Replace("\n", "\r\n");
        }
    }
}

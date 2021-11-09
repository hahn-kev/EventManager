using System;
using System.Linq;
using System.Security;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Text;
using AngleSharp.Xml;

namespace EventCore
{
    public class FtlXmlMarkupFormatter : XmlMarkupFormatter
    {
        public new static readonly IMarkupFormatter Instance = new FtlXmlMarkupFormatter();

        /// <summary>
        /// Creates a new instance of the pretty markup formatter.
        /// </summary>
        public FtlXmlMarkupFormatter()
        {
            _indentCount = 0;
            _indentString = "\t";
            _newLineString = "\n";
        }

        public override string Comment(IComment comment)
        {
            return "<!--" + NormalizeLineEndings(comment.Data) + "-->";
        }

        private string NormalizeLineEndings(string s)
        {
            return s.Replace("\n", "\r\n");
        }


        private string _indentString;
        private string _newLineString;
        private int _indentCount;


        /// <summary>
        /// Gets or sets the indentation string.
        /// </summary>
        public string Indentation
        {
            get => _indentString;
            set => _indentString = value;
        }

        /// <summary>
        /// Gets or sets the newline string.
        /// </summary>
        public string NewLine
        {
            get => _newLineString;
            set => _newLineString = value;
        }


        /// <inheritdoc />
        public override string Doctype(IDocumentType doctype)
        {
            var before = string.Empty;

            if (doctype.ParentElement != null)
            {
                before = IndentBefore();
            }

            return before + base.Doctype(doctype) + NewLine;
        }

        /// <inheritdoc />
        public override string Processing(IProcessingInstruction processing) =>
            IndentBefore() + base.Processing(processing);

        /// <inheritdoc />
        public override string Text(ICharacterData text)
        {
            var content = text.Data;
            var before = string.Empty;
            var singleLine = content.Replace(Symbols.LineFeed, Symbols.Space);

            if (text.NextSibling is ICharacterData == false)
            {
                singleLine = singleLine.TrimEnd();
            }

            if (singleLine.Length > 0 && text.PreviousSibling is ICharacterData == false &&
                singleLine[0].IsSpaceCharacter())
            {
                singleLine = singleLine.TrimStart();
                before = IndentBefore();
            }

            return before + XmlMarkupFormatter.EscapeText(NormalizeLineEndings(singleLine));
        }

        /// <inheritdoc />
        public override string OpenTag(IElement element, bool selfClosing)
        {
            var before = string.Empty;
            var previousSibling = element.PreviousSibling as IText;

            if (element.ParentElement != null && (previousSibling is null || EndsWithSpace(previousSibling)))
            {
                before = IndentBefore();
            }

            _indentCount++;
            return before + base.OpenTag(element, !element.HasChildNodes && selfClosing);
        }

        /// <inheritdoc />
        public override string CloseTag(IElement element, bool selfClosing)
        {
            _indentCount--;
            var before = string.Empty;
            var lastChild = element.LastChild as IText;

            if (element.HasChildNodes && (lastChild is null || EndsWithSpace(lastChild)))
            {
                before = IndentBefore();
            }

            return before + base.CloseTag(element, !element.HasChildNodes && selfClosing);
        }

        private static bool EndsWithSpace(ICharacterData text)
        {
            var content = text.Data;
            return content.Length > 0 && content[content.Length - 1].IsSpaceCharacter();
        }

        private string IndentBefore() =>
            _newLineString + string.Join(string.Empty, Enumerable.Repeat(_indentString, _indentCount));
    }
}

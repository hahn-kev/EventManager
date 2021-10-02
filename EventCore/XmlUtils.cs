using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AngleSharp.Dom;

namespace EventCore
{
    internal static class XmlUtils
    {
        public static IEnumerable<XNode> ParseFragment(XmlReader xr)
        {
            xr.MoveToContent();
            XNode node;
            while (!xr.EOF && (node = XNode.ReadFrom(xr)) != null)
            {
                yield return node;
            }
        }

        public static IElement? Element(this IElement element, string tagName)
        {
            return element.Children.FirstOrDefault(e => e.TagName == tagName);
        }
    }
}

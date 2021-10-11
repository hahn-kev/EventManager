﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AngleSharp.Dom;

namespace EventCore
{
    public static class XmlUtils
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
            return element.Children.FirstOrDefault(e => e.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        }

        public static void SetChildElementText(this IElement element,
            string tagName,
            string value,
            bool autoCreate = false,
            bool prependNew = false)
        {
            var child = element.Element(tagName);
            if (child != null) child.TextContent = value ?? "";
            else if (!autoCreate) return;

            if (prependNew) PrependNew(element, tagName, value);
            else AppendNew(element, tagName, value);
        }

        public static void ToggleChildElement(this IElement element, string tagName, bool value)
        {
            if (value)
            {
                element.AppendNew(tagName);
            }
            else if (element.Element(tagName) is {} child)
            {
                element.RemoveChild(child);
            }
        }

        public static IElement AppendNew(this IElement element, string tagName, string? textContent = null)
        {
            var childElement = NewElement(element, tagName, textContent);
            element.AppendChild(childElement);
            return childElement;
        }

        private static IElement NewElement(IElement element, string tagName, string? textContent)
        {
            var childElement = element.Owner!.CreateElement(tagName);
            if (textContent != null) childElement.TextContent = textContent;
            return childElement;
        }

        public static IElement PrependNew(this IElement element, string tagName, string? textContent = null)
        {
            var childElement = NewElement(element, tagName, textContent);
            if (element.FirstChild != null)
            {
                element.FirstChild.InsertBefore(childElement);
            }
            else
            {
                element.AppendChild(childElement);
            }
            return childElement;
        }
    }
}

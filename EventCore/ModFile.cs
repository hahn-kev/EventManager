using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Xml.Dom;
using EventCore.FTL;

namespace EventCore
{
    public class ModFile
    {
        public ModRoot? ModRoot { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public bool Dirty { get; private set; }
        public Dictionary<string, FTLEvent> Events { get; }
        public Dictionary<string, FTLTextRef> TextRefs { get; } = new();
        public List<ICanHaveTextRef> AllCanRefTexts { get; } = new();

        private IDocument? _document;

        public IDocument Document
        {
            get => _document ?? throw new NullReferenceException("document is null");
            set
            {
                _document = value;
                SetupDirtyWatch();
            }
        }

        public ModFile(string filePath)
        {
            FilePath = filePath;
            Events = new Dictionary<string, FTLEvent>();
        }

        private void SetupDirtyWatch()
        {
            if (_document == null) return;
            var observer = new MutationObserver((mutations, observer) =>
            {
                Dirty = true;
                observer.Disconnect();
            });
            observer.Connect(_document.DocumentElement, true, true, true, true);
        }

        public FTLEvent AddEvent()
        {
            var parent = Events.Values.FirstOrDefault()?.Element.ParentElement;
            if (parent == null)
            {
                parent = Document.QuerySelector("FTL");
            }

            if (parent == null)
                throw new NullReferenceException("unable to find parent element to add the new event element too");

            return FTLEvent.NewEvent(parent, this);
        }

        public void EventNameUpdated(string? oldName, string? newName, FTLEvent ftlEvent)
        {
            if (oldName == newName) return;
            oldName = string.IsNullOrEmpty(oldName) ? null : oldName;
            newName = string.IsNullOrEmpty(newName) ? null : newName;

            if (oldName != null)
            {
                Events.Remove(oldName);
                ModRoot?.EventsLookup.Remove(oldName);
            }

            if (newName != null)
            {
                Events[newName] = ftlEvent;
                if (ModRoot != null) ModRoot.EventsLookup[newName] = ftlEvent;
            }
        }
    }
}

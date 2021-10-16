﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Xml.Dom;

namespace EventCore
{
    public class ModFile
    {
        public ModRoot? ModRoot { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public bool Dirty { get; private set; }
        public Dictionary<string, FTLEvent> Events { get; }

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

        public ModFile(string filePath)
        {
            FilePath = filePath;
            Events = new Dictionary<string, FTLEvent>();
        }

        public FTLEvent AddEvent()
        {
            var firstEvent = Events.Values.FirstOrDefault();
            if (firstEvent == null) throw new NullReferenceException("no event found in mod file " + FileName);
            var newEventElement = firstEvent.Element.ParentElement.AppendNew("event");
            var ftlEvent = new FTLEvent(newEventElement, "NEW_EVENT", new List<FTLChoice>(), this);
            ftlEvent.Name = ftlEvent.Name;
            // Events[ftlEvent.Name!] = ftlEvent;
            // if (ModRoot != null) ModRoot.EventsLookup[ftlEvent.Name!] = ftlEvent;
            return ftlEvent;
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

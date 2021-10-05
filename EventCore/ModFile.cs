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

        private IXmlDocument? _document;

        public IXmlDocument? Document
        {
            get => _document;
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
    }
}

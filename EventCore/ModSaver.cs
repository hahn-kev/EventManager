using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Xml;

namespace EventCore
{
    public class ModSaver
    {
        public async Task Save(ModRoot modRoot)
        {
            var modFiles = modRoot.ModFiles.Values;
            await Task.WhenAll(modFiles.Where(file => file.Dirty).Select(SaveFile));
        }

        public async Task SaveFile(ModFile file)
        {
            await using var fileStream = File.Open(file.FilePath, FileMode.Truncate, FileAccess.Write, FileShare.Write);
            await SaveFile(file, fileStream);
        }

        public async Task SaveFile(ModFile file, Stream destination)
        {
            if (file.Document == null) throw new NullReferenceException("mod file is not loaded");
            await using var sr = new StreamWriter(destination);
            file.Document.ToHtml(sr, FtlXmlMarkupFormatter.Instance);
            await sr.FlushAsync();
        }
    }
}

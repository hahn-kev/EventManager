using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using EventCore;
using Material.Styles;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private ModRoot? _modRoot;

        public MainWindowViewModel(ILogger logger)
        {
            _logger = logger;
            EventsList.ObserveSelectedEvent.Subscribe(ftlEvent => EditorTreeViewModel.SetTopLevelEvent(ftlEvent));
        }

        public string WindowTitle => "Event Manager " + AppVersion.Get();

        public EventsListViewModel EventsList { get; } = new();
        public EventEditorTreeViewModel EditorTreeViewModel { get; } = new();

        public ModRoot? ModRoot
        {
            get => _modRoot;
            set => this.RaiseAndSetIfChanged(ref _modRoot, value);
        }

        private ModFile? _selectedModFile;

        public ModFile? SelectedModFile
        {
            get => _selectedModFile;
            set => this.RaiseAndSetIfChanged(ref _selectedModFile, value);
        }

        public void AddEvent()
        {
            if (SelectedModFile == null)
            {
                return;
            }

            var ftlEvent = SelectedModFile.AddEvent();
            EventsList.Filter = "";
            EventsList.SetSelectedEvent(ftlEvent);
        }

        public async Task Load(Window window)
        {
            string? folderPath = null;
#if DEBUG
            // folderPath = @"D:\Games\FTL Stuff\EventManager\Tests\TestData\data";
#endif
            try
            {
                if (folderPath == null)
                {
                    var folderDialog = new OpenFolderDialog { Title = "Select the mod data folder" };
                    folderPath = await folderDialog.ShowAsync(window);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error opening folder dialog");
                throw;
            }


            if (string.IsNullOrEmpty(folderPath)) return;
            var modLoader = new ModLoader(folderPath);
            try
            {
                ModRoot = modLoader.Load();
                SelectedModFile = ModRoot.ModFiles.Values.FirstOrDefault();
            }
            catch (Exception e)
            {
                SnackbarHost.Post("Error: " + e.Message);
                _logger.LogError(e, "Error loading mod: " + folderPath);
                return;
            }

            GC.Collect();
            EventsList.EventsLoaded(ModRoot.TopLevelEvents.ToArray());
        }

        public async Task Save()
        {
            if (ModRoot == null) return;
            var modSaver = new ModSaver();
            await modSaver.Save(ModRoot);
        }
    }
}

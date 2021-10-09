using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventCore;

namespace EventManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ModRoot? _modRoot;

        public MainWindowViewModel()
        {
            EventsList.ObserveSelectedEvent.Subscribe(ftlEvent => EditorTreeViewModel.SetTopLevelEvent(ftlEvent));
        }

        public EventsListViewModel EventsList { get; } = new();
        public EventEditorTreeViewModel EditorTreeViewModel { get; } = new();

        public async Task Load()
        {
            var modLoader = new ModLoader(@"D:\Games\FTL Stuff\EventManager\Tests\TestData\data");

            _modRoot = await modLoader.Load();
            GC.Collect();
            EventsList.Root.OnNext(_modRoot);
        }

        public async Task Save()
        {
            if (_modRoot == null) return;
            var modSaver = new ModSaver();
            await modSaver.Save(_modRoot);
        }
    }
}

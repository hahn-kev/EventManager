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
        public MainWindowViewModel()
        {
            HasSelectedEvent = Observable.Return(false).Concat(EventsList.ObserveSelectedEvent.FirstAsync().Select(_ => true));
            EventsList.ObserveSelectedEvent.Subscribe(ftlEvent => EditorTreeViewModel.SetTopLevelEvent(ftlEvent));
        }

        public EventsListViewModel EventsList { get; } = new EventsListViewModel();
        public IObservable<bool> HasSelectedEvent { get; }
        public EventEditorTreeViewModel EditorTreeViewModel { get; } = new EventEditorTreeViewModel();

        public async Task Load()
        {
            var modLoader = new ModLoader(@"D:\Games\FTL Stuff\EventManager\Tests\TestData\data");
            EventsList.Root.OnNext(await modLoader.Load());
        }
    }
}

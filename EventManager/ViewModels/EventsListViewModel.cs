using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Layout;
using DynamicData.Binding;
using EventCore;
using EventCore.FTL;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventsListViewModel : ViewModelBase
    {
        public EventsListViewModel()
        {
            ObserveSelectedEvent = this.WhenValueChanged(model => model.SelectedEvent)
                .Where(@event => @event != null)!.OfType<FTLEvent>();
            var filterRx = this.WhenValueChanged(vm => vm.Filter);
            Events = Root.CombineLatest(filterRx)
                .Select(t =>
                {
                    var (modRoot, filter) = t;
                    if (modRoot == null) return Array.Empty<FTLEvent>();
                    return modRoot.TopLevelEvents.Where(e =>
                        string.IsNullOrWhiteSpace(filter) ||
                        (e.Name?.StartsWith(filter, true, null) ?? false));
                });
            //first returns false until an event get's selected
            HasSelectedEvent = Observable.Return(false)
                .Concat(this.WhenValueChanged(model => model.SelectedEvent).Select(ftlEvent => ftlEvent != null));
        }

        public BehaviorSubject<ModRoot?> Root { get; } = new(null);
        private string? _filter;

        public string? Filter
        {
            get => _filter;
            set => this.RaiseAndSetIfChanged(ref _filter, value);
        }


        private FTLEvent? _selectedEvent;

        public FTLEvent? SelectedEvent
        {
            get => _selectedEvent;
            set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
        }

        public IObservable<bool> HasSelectedEvent { get; }
        public IObservable<FTLEvent> ObserveSelectedEvent { get; }

        public IObservable<IEnumerable<FTLEvent>> Events { get; }
    }
}

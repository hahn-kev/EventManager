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
        public EventsListViewModel(FTLEventList eventList): this()
        {
            EventsLoaded(eventList.FtlEvents);
        }

        public EventsListViewModel()
        {
            ObserveSelectedEvent = this.WhenValueChanged(model => model.SelectedEvent)
                .Where(@event => @event != null)!.OfType<FTLEvent>();
            var filterRx = this.WhenValueChanged(vm => vm.Filter);
            Events = _allEvents.CombineLatest(filterRx)
                .Select(t =>
                {
                    var (ftlEvents, filter) = t;
                    return ftlEvents.Where(e =>
                        string.IsNullOrWhiteSpace(filter) ||
                        (e.Name?.StartsWith(filter, true, null) ?? false));
                });
            //first returns false until an event get's selected
            HasSelectedEvent = Observable.Return(false)
                .Concat(this.WhenValueChanged(model => model.SelectedEvent).Select(ftlEvent => ftlEvent != null));
        }

        public void EventsLoaded(ICollection<FTLEvent> events)
        {
            _allEvents.OnNext(events);
        }

        private readonly BehaviorSubject<ICollection<FTLEvent>> _allEvents = new(Array.Empty<FTLEvent>());
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

        private bool _showFilter = true;

        public bool ShowFilter
        {
            get => _showFilter;
            set => this.RaiseAndSetIfChanged(ref _showFilter, value);
        }

        private bool _showIndex = false;
        public bool ShowIndex
        {
            get => _showIndex;
            set => this.RaiseAndSetIfChanged(ref _showIndex, value);
        }

        public IObservable<bool> HasSelectedEvent { get; }
        public IObservable<FTLEvent> ObserveSelectedEvent { get; }

        public IObservable<IEnumerable<FTLEvent>> Events { get; }
    }
}

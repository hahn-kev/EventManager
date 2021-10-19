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
        public string? Title { get; set; }
        public EventsListViewModel(FTLEventList eventList) : this()
        {
            EventsLoaded(eventList.FtlEvents);
        }

        public EventsListViewModel()
        {
            ObserveSelectedEvent = this.WhenValueChanged(model => model.SelectedEvent)
                .Where(@event => @event != null).Select(model => model!.Event);
            var filterRx = this.WhenValueChanged(vm => vm.Filter);
            Events = _allEvents.CombineLatest(filterRx)
                .Select(t =>
                {
                    var (ftlEvents, filter) = t;
                    return ftlEvents.Where(e =>
                            string.IsNullOrWhiteSpace(filter) ||
                            (e.Name?.StartsWith(filter, true, null) ?? false))
                        .Select((ftlEvent, index) => new EventItemViewModel(ftlEvent, index, this));
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

        private EventItemViewModel? _selectedEvent;

        public EventItemViewModel? SelectedEvent
        {
            get => _selectedEvent;
            set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
        }

        public void SetSelectedEvent(FTLEvent ftlEvent)
        {
            SelectedEvent = new EventItemViewModel(ftlEvent, _allEvents.Value.Count, this);
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

        public IObservable<IEnumerable<EventItemViewModel>> Events { get; }
    }

    public class EventItemViewModel : ViewModelBase
    {
        public FTLEvent Event { get; }
        private readonly int _index;
        private readonly EventsListViewModel _parent;

        public EventItemViewModel(FTLEvent @event, int index, EventsListViewModel parent)
        {
            Event = @event;
            _index = index;
            _parent = parent;
        }

        public string? Title
        {
            get
            {
                if (!_parent.ShowIndex)
                    return Event.Name;
                if (Event.Name == null)
                    return $"{_index + 1}";
                return $"{_index + 1} {Event.Name}";
            }
        }
    }
}

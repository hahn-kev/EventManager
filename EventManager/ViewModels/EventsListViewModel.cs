using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Layout;
using DynamicData.Binding;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventsListViewModel : ViewModelBase
    {
        public EventsListViewModel()
        {
        }

        public BehaviorSubject<ModRoot?> Root { get; } = new(null);
        public BehaviorSubject<string> FilterObservable { get; } = new("");

        public string Filter
        {
            get => FilterObservable.Value;
            set => FilterObservable.OnNext(value);
        }


        private FTLEvent? _selectedEvent;

        public FTLEvent? SelectedEvent
        {
            get => _selectedEvent;
            set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
        }

        public IObservable<FTLEvent> ObserveSelectedEvent =>
            this.WhenValueChanged(model => model.SelectedEvent).Where(@event => @event != null)!.OfType<FTLEvent>();

        public IObservable<IEnumerable<FTLEvent>> Events =>
            Root.CombineLatest(FilterObservable)
                .Select(t =>
                {
                    var (modRoot, filter) = t;
                    if (modRoot == null) return Array.Empty<FTLEvent>();
                    return modRoot.TopLevelEvents.Where(e =>
                        string.IsNullOrWhiteSpace(filter) ||
                        (e.Name?.StartsWith(filter, true, null) ?? false));
                });
    }
}

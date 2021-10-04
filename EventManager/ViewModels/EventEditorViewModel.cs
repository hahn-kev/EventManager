using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using DynamicData.Binding;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorViewModel : ViewModelBase
    {
        public EventEditorViewModel()
        {
        }

        public EventEditorViewModel(FTLEvent @event)
        {
            Event = @event;
        }

        public Subject<Unit> Closed { get; } = new();

        public void Close()
        {
            Closed.OnNext(Unit.Default);
            Closed.OnCompleted();
        }

        private FTLEvent Event { get; }

        public string? Name
        {
            get => Event.Name;
            set
            {
                this.RaisePropertyChanging();
                Event.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public string FileName => Event.ModFile.FileName;

        public string? Text
        {
            get => Event.Text;
            set
            {
                this.RaisePropertyChanging();
                Event.Text = value;
                this.RaisePropertyChanged();
            }
        }

        public List<FTLChoice> Choices => Event.Choices;
        public bool HasChoices => Choices.Count > 0;

        private FTLChoice? _selectedChoice;

        public FTLChoice? SelectedChoice
        {
            get => _selectedChoice;
            set => this.RaiseAndSetIfChanged(ref _selectedChoice, value);
        }

        public IObservable<string?> RawText =>
            Observable.Return(this).Concat(this.WhenAnyPropertyChanged())
                .Select(model => model?.Event.Element.OuterHtml);

        public void SetRawText(string rawText)
        {
            Event.Element.OuterHtml = rawText;
        }
    }
}

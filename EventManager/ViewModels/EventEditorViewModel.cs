using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using Avalonia.Metadata;
using DynamicData.Binding;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorViewModel : ViewModelBase
    {
        public EventEditorViewModel()
        {
            SelectedChoiceVm = this.ObservableForProperty(model => model.SelectedChoice).Select(change =>
                change.Value == null ? null : new ChoiceEditorViewModel(change.Value));
        }

        public EventEditorViewModel(FTLEvent @event) : this()
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
        public bool IsEventRef => Event.IsRef;
        public FTLEventRef? EventRef => Event as FTLEventRef;

        private FTLChoice? _selectedChoice;

        public FTLChoice? SelectedChoice
        {
            get => _selectedChoice;
            set { this.RaiseAndSetIfChanged(ref _selectedChoice, value); }
        }

        public IObservable<ChoiceEditorViewModel?> SelectedChoiceVm { get; }

        public IObservable<string?> RawText =>
            Observable.Return(this).Concat(this.WhenAnyPropertyChanged())
                .Select(model => model?.Event.Element.OuterHtml);

        public void SetRawText(string rawText)
        {
            Event.Element.OuterHtml = rawText;
        }

        public FTLEvent.QuestModeEnum[] QuestModes { get; } = new[]
            { FTLEvent.QuestModeEnum.None, FTLEvent.QuestModeEnum.Start, FTLEvent.QuestModeEnum.Define };

        public FTLEvent.QuestModeEnum QuestMode
        {
            get => Event.QuestMode;
            set
            {
                this.RaisePropertyChanging();
                Event.QuestMode = value;
                this.RaisePropertyChanged();
            }
        }

        [DependsOn(nameof(QuestMode))]
        public bool HasQuestDefinition => Event.QuestMode == FTLEvent.QuestModeEnum.Define;

        [DependsOn(nameof(QuestMode))]
        public bool HasQuestStart => Event.QuestMode == FTLEvent.QuestModeEnum.Start;
    }
}

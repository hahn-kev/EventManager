using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using Avalonia.Metadata;
using DynamicData;
using DynamicData.Binding;
using EventCore;
using Material.Styles;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorViewModel : ViewModelBase
    {
        public EventEditorViewModel()
        {
        }

        public EventEditorViewModel(FTLEvent @event) : this()
        {
            Event = @event;
            Choices = new(Event.Choices.Select(c => new ChoiceEditorViewModel(c)));
            this.WhenValueChanged(model => model.SelectedChoice)
                .Where(model => model != null)
                .Subscribe(model => OpenEvent.OnNext(model!.Event));
        }

        public Subject<FTLEvent> OpenEvent { get; } = new();

        public Subject<Unit> Closed { get; } = new();

        public void Close()
        {
            Closed.OnNext(Unit.Default);
            Closed.OnCompleted();
        }

        private FTLEvent Event { get; }

        public bool HasReward => Event.HasCrew || Event.HasReward || !string.IsNullOrEmpty(Event.WeaponReward);

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

        public bool IsEventRef => Event.IsRef;
        public FTLEventRef? EventRef => Event as FTLEventRef;

        public void EditEventRef()
        {
            if (EventRef == null) return;
            if (EventRef is { ActualEvent: { } } eventRef)
                OpenEvent.OnNext(eventRef.ActualEvent);
            else
            {
                SnackbarHost.Post($"Unknown Event Name: {EventRef.RefName}");
            }
        }

        public ObservableCollection<ChoiceEditorViewModel> Choices { get; }
        public bool HasChoices => Choices.Count > 0;

        private ChoiceEditorViewModel? _selectedChoice;

        public ChoiceEditorViewModel? SelectedChoice
        {
            get => _selectedChoice;
            set => this.RaiseAndSetIfChanged(ref _selectedChoice, value);
        }

        public void NewChoice()
        {
            var newChoice = Event.AddNewChoice();
            Choices.Add(new ChoiceEditorViewModel(newChoice));
        }

        public IObservable<string?> RawText =>
            Observable.Return(this).Concat(this.WhenAnyPropertyChanged())
                .Select(model => model?.Event.Element.OuterHtml);

        public bool RefreshRawTextHack
        {
            //set is called whenever the xml text expander changes
            set
            {
                //only raise if true, this will cause RawText above to emit the most recent Xml
                if (value)
                {
                    this.RaisePropertyChanged();
                }
            }
        }

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

        public IObservable<bool> HasQuest =>
            this.WhenValueChanged(model => model.QuestMode).Select(mode => mode != FTLEvent.QuestModeEnum.None);

        public IObservable<bool> HasQuestDefinition =>
            this.WhenValueChanged(model => model.QuestMode)
                .Select(mode => mode == FTLEvent.QuestModeEnum.Define);

        public IObservable<bool> HasQuestStart =>
            this.WhenValueChanged(model => model.QuestMode)
                .Select(mode => mode == FTLEvent.QuestModeEnum.Start);

        public bool HasShip
        {
            get => Event.HasShip;
            set
            {
                this.RaisePropertyChanging();
                Event.HasShip = value;
                this.RaisePropertyChanged();
            }
        }
    }
}

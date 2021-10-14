using System.Collections.Generic;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class ChoiceEditorViewModel : ViewModelBase
    {
        private FTLChoice FtlChoice { get; }

        public enum EventImpType
        {
            Inline,
            Load,
            Unknown
        }

        public ChoiceEditorViewModel()
        {
        }

        public ChoiceEditorViewModel(FTLChoice ftlChoice)
        {
            FtlChoice = ftlChoice;
        }

        public bool Hidden
        {
            get => FtlChoice.Hidden;
            set => FtlChoice.Hidden = value;
        }

        public string Text
        {
            get => FtlChoice.Text;
            set
            {
                this.RaisePropertyChanging();
                FtlChoice.Text = value;
                this.RaisePropertyChanged();
            }
        }

        public string? EventName
        {
            get => FtlChoice.Event.Name;
            set => FtlChoice.Event.Name = value;
        }

        public FTLEvent Event => FtlChoice.Event;

        public EventImpType[] EventTypes { get; } = new[] { EventImpType.Load, EventImpType.Inline };
        public bool EnableEventTypeSwitching => FtlChoice.Event.IsRef;
        public EventImpType EventType
        {
            get => FtlChoice.Event.IsRef ? EventImpType.Load : EventImpType.Inline;
            set
            {
                if (value == EventType) return;
                if (value == EventImpType.Load)
                {
                    //switch to load
                    FtlChoice.ConvertToLoadEvent();
                    return;
                }

                if (value == EventImpType.Inline)
                {
                    //switch to inline, don't inline current event make new empty one
                    FtlChoice.ConvertToInlineEvent();
                }
            }
        }

        public bool ShowEventNameEditor => FtlChoice.Event.IsRef;

        public string? Requirement
        {
            get => FtlChoice.Requirement;
            set => FtlChoice.Requirement = value;
        }

        public int RequirementLevel
        {
            get => FtlChoice.RequirementLevel;
            set => FtlChoice.RequirementLevel = value;
        }
    }
}

using EventCore;

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
            set => FtlChoice.Text = value;
        }

        public string? EventName
        {
            get => FtlChoice.Event.Name;
            set => FtlChoice.Event.Name = value;
        }

        public EventImpType[] EventTypes { get; } = new[] { EventImpType.Load, EventImpType.Inline };
        public EventImpType EventType
        {
            get => FtlChoice.Event.IsRef ? EventImpType.Load : EventImpType.Inline;
            set
            {
                //todo
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

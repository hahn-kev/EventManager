using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Input;
using DynamicData.Binding;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorViewModel : ViewModelBase
    {
        public EventEditorViewModel(FTLEvent @event)
        {
            Event = @event;
            Name = Event.Name;
            Text = Event.Text;
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

        public IObservable<string?> RawText =>
            Observable.Return(this).Concat(this.WhenAnyPropertyChanged())
                .Select(model => model?.Event.Element.OuterHtml);

         public void SetRawText(string rawText)
         {
             Event.Element.OuterHtml = rawText;
         }
    }
}

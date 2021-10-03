using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Layout;
using EventCore;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorTreeViewModel : ViewModelBase
    {
        public ObservableCollection<EventEditorViewModel> EditorViewModels { get; } = new();
        private Subject<int> EventsCleared = new();

        public void SetTopLevelEvent(FTLEvent ftlEvent)
        {
            RemoveEditorsTo(0);
            AddNewEventOnEnd(ftlEvent);
        }

        private void AddNewEventOnEnd(FTLEvent ftlEvent)
        {
            var choiceDepth = EditorViewModels.Count;
            var editorViewModel = new EventEditorViewModel(ftlEvent);
            editorViewModel.ObservableForProperty(model => model.SelectedChoice)
                .Where(change => change.Value != null)
                .TakeUntil(EventsCleared.Where(clearedDepth => clearedDepth < choiceDepth))
                .Subscribe(change => ChoiceChanged(change.Value!.Event, choiceDepth + 1));
            EditorViewModels.Add(editorViewModel);
        }

        public void ChoiceChanged(FTLEvent ftlEvent, int depth)
        {
            RemoveEditorsTo(depth);
            AddNewEventOnEnd(ftlEvent);
        }

        private void RemoveEditorsTo(int depth)
        {
            if (depth > 0)
            {
                while (EditorViewModels.Count > depth)
                    EditorViewModels.RemoveAt(depth);
            }
            else
            {
                EditorViewModels.Clear();
            }

            EventsCleared.OnNext(depth);
        }


        public AttachedLayout RepeaterLayout { get; } = new StackLayout
        {
            Orientation = Orientation.Horizontal
        };
    }
}

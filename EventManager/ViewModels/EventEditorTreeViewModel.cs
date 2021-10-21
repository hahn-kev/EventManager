using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Layout;
using EventCore;
using EventCore.FTL;
using ReactiveUI;

namespace EventManager.ViewModels
{
    public class EventEditorTreeViewModel : ViewModelBase
    {
        public ObservableCollection<ViewModelBase> EditorViewModels { get; } = new();
        private readonly Subject<int> _eventsCleared = new();

        public void SetTopLevelEvent(FTLEvent ftlEvent)
        {
            RemoveEditorsTo(0);
            AddNewEventOnEnd(ftlEvent);
        }

        private void AddNewEventOnEnd(FTLEvent ftlEvent)
        {
            var choiceDepth = EditorViewModels.Count;
            var shouldClose = _eventsCleared.Where(clearedDepth => clearedDepth < choiceDepth);
            ViewModelBase viewModel;
            switch (ftlEvent)
            {
                case FTLEventList eventList:
                    var eventsListViewModel = new EventsListViewModel(eventList)
                    {
                        ShowFilter = false,
                        ShowIndex = true,
                        ShowAddButton = true,
                        Title = eventList.Name
                    };
                    eventsListViewModel.AddEvent.TakeUntil(shouldClose)
                        .Subscribe(_ =>
                        {
                            eventList.AddNewEvent();
                            eventsListViewModel.EventsLoaded(eventList.FtlEvents);
                        });

                    eventsListViewModel.ObserveSelectedEvent.TakeUntil(shouldClose)
                        .Subscribe(newOpenedEvent => OpenEvent(newOpenedEvent, choiceDepth + 1));
                    viewModel = eventsListViewModel;
                    break;
                default:
                    var editorViewModel = new EventEditorViewModel(ftlEvent);
                    viewModel = editorViewModel;
                    editorViewModel.OpenEvent
                        .TakeUntil(shouldClose)
                        .Subscribe(newOpenedEvent => OpenEvent(newOpenedEvent, choiceDepth + 1));

                    editorViewModel.Closed.Subscribe(_ =>
                    {
                        EditorClosed(choiceDepth);
                    });
                    break;
            }


            EditorViewModels.Add(viewModel);

            // if (ftlEvent is FTLEventRef { ActualEvent: { } } eventRef)
            //     AddNewEventOnEnd(eventRef.ActualEvent);
        }

        private void EditorClosed(int choiceDepth)
        {
            RemoveEditorsTo(choiceDepth);
            var lastEditor = EditorViewModels.LastOrDefault() as EventEditorViewModel;
            if (lastEditor != null) lastEditor.SelectedChoice = null;
        }

        public void OpenEvent(FTLEvent ftlEvent, int depth)
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

            _eventsCleared.OnNext(depth);
        }
    }
}

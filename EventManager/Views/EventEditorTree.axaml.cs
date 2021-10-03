using System;
using System.Reactive.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DynamicData.Binding;
using EventManager.ViewModels;
using ReactiveUI;

namespace EventManager.Views
{
    public class EventEditorTree : UserControl
    {
        public EventEditorTree()
        {
            InitializeComponent();

            this.ObservableForProperty(tree => tree.DataContext).FirstAsync().Subscribe(change => SetupScrollToEnd());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SetupScrollToEnd()
        {
            var scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");
            var treeViewModel = DataContext as EventEditorTreeViewModel;
            if (treeViewModel == null) return;
            treeViewModel.EditorViewModels.ObserveCollectionChanges()
                .Delay(TimeSpan.FromTicks(1), AvaloniaScheduler.Instance)
                .Subscribe(_ => scrollViewer.PageRight());
        }
    }
}

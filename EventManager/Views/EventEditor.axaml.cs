using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EventCore;
using EventCore.FTL;
using EventManager.ViewModels;

namespace EventManager.Views
{
    public class EventEditor : UserControl
    {
        public EventEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RemoveDamage(object? sender, RoutedEventArgs e)
        {
            var ftlDamage = (sender as Control)?.DataContext as FTLDamage;
            var eventEditorViewModel = DataContext as EventEditorViewModel;
            if (ftlDamage == null)
                throw new NullReferenceException("click sender does not have ftl damage data context");
            if (eventEditorViewModel == null)
                throw new NullReferenceException("data context is not event editor view model");
            eventEditorViewModel.RemoveDamage(ftlDamage);
        }
    }
}

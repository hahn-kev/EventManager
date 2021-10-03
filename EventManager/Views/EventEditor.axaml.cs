using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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

        private void InputElement_OnTextInput(object? sender, TextInputEventArgs e)
        {
            var eventEditorViewModel = DataContext as EventEditorViewModel;
            eventEditorViewModel?.SetRawText(e.Text ?? "");
        }
    }
}

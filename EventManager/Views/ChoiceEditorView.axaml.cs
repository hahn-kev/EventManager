using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EventManager.Views
{
    public class ChoiceEditorView : UserControl
    {
        public ChoiceEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
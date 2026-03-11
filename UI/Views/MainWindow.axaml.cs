using Avalonia.Controls;

namespace UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Width = 700;
            Height = 700;
        }

        private void OnGraphClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CanvasTabControl.SelectedIndex = 0;
        }

        private void OnTableClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CanvasTabControl.SelectedIndex = 1;
        }
    }
}
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HotkeyManager.Services;
using HotkeyManager.ViewModels;
using SharpHook;

namespace HotkeyManager
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            Closing += OnClosing; // Підписуємося на закриття
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnClosing(object sender, WindowClosingEventArgs e)
        {
            _viewModel.Dispose(); // Викликаємо Dispose при закритті
        }
    }
}
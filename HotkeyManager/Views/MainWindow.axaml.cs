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
     

        public MainWindow()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        
    }
}
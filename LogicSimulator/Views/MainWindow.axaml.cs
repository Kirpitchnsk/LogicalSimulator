using Avalonia.Controls;
using LogicSimulator.ViewModels;

namespace LogicSimulator.Views {
    public partial class MainWindow: Window {
        readonly MainWindowViewModel mwvm;

        public MainWindow() {
            InitializeComponent();
            mwvm = new MainWindowViewModel();
            DataContext = mwvm;
            MainWindowViewModel.AddWindow(this);
        }

        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            mwvm.DTapped(sender, e);
        }
    }
}
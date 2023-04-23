using Avalonia.Controls;
using LogicSimulator.ViewModels;

namespace LogicSimulator.Views {
    public partial class MainWindow: Window {
        MainWindowViewModel mwvm;

        public MainWindow() {
            InitializeComponent();
            mwvm = new MainWindowViewModel();
            DataContext = mwvm;
            mwvm.AddWindow(this);
        }

        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            mwvm.DTapped(sender, e);
        }
    }
}
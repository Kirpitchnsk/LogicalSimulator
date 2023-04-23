using Avalonia.Controls.Presenters;
using Avalonia.Controls;

namespace LogicSimulator.ViewModels {
    public class LauncherWindowViewModel: ViewModelBase {
        public string[] ProjectList { get => new string[] { "1", "2", "3" }; }



        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var src = (Control?) e.Source;

            if (src is ContentPresenter cp && cp.Child is Border bord) src = bord;
            if (src is Border bord2 && bord2.Child is TextBlock tb2) src = tb2;


        }
    }
}
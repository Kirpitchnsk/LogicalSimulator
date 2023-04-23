using Avalonia.Controls.Presenters;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;
using LogicSimulator.Views;

namespace LogicSimulator.ViewModels {
    public class LauncherWindowViewModel: ViewModelBase {
        Window? me;
        readonly Window mw = new MainWindow();

        public LauncherWindowViewModel() {
            Create = ReactiveCommand.Create<Unit, Unit>(_ => { FuncCreate(); return new Unit(); });
            Exit = ReactiveCommand.Create<Unit, Unit>(_ => { FuncExit(); return new Unit(); });
        }
        public void AddWindow(Window lw) => me = lw;

        void FuncCreate() {
            var newy = map.filer.CreateProject();
            current_proj = newy;
            mw.Show();
            me?.Close();
        }
        void FuncExit() {
            me?.Close();
        }

        public ReactiveCommand<Unit, Unit> Create { get; }
        public ReactiveCommand<Unit, Unit> Exit { get; }


        public static string[] ProjectList { get => new string[] { "1", "2", "3" }; }



        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var src = (Control?) e.Source;

            if (src is ContentPresenter cp && cp.Child is Border bord) src = bord;
            if (src is Border bord2 && bord2.Child is TextBlock tb2) src = tb2;


        }
    }
}
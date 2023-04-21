using Avalonia.Controls;
using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;

namespace LogicSimulator.Views {
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            var mwvm = new MainWindowViewModel();
            DataContext = mwvm;
            mwvm.AddWindow(this);

            var ctx = this.Find<Canvas>("Canvas");
            var gate = new AND_2();
            gate.Move(new(100, 100));
            ctx.Children.Add(gate);
        }
    }
}
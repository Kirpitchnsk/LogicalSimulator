using Avalonia.Controls;
using Avalonia.LogicalTree;
using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;

namespace LogicSimulator.Views {
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            var ctx = this.Find<Canvas>("Canvas");
            var gate = new AND_2();
            gate.Move(new(100, 100));
            ctx.Children.Add(gate);
        }
    }
}
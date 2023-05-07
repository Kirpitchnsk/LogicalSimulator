using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class LightBulb: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 7;

        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;
        protected override int[][] Sides => new int[][] {
            System.Array.Empty<int>(),
            new int[] { 0 },
            System.Array.Empty<int>(),
            System.Array.Empty<int>()
        };

        protected override void Init() {
            width = 30 * 2.5;
            height = 30 * 2.5;
            InitializeComponent();
            DataContext = this;
        }

        readonly Border border;
        public LightBulb(): base() {
            if (LogicalChildren[0].LogicalChildren[1] is not Border b) throw new Exception("Такого не бывает");
            border = b;
        }

        /*
         * Мозги
         */

        readonly SolidColorBrush ColorA = new(Color.Parse("#00ff00")); // On
        readonly SolidColorBrush ColorB = new(Color.Parse("#1c1c1c")); // Off
        public void Brain(ref bool[] ins, ref bool[] outs) {
            var value = ins[0];
            Dispatcher.UIThread.InvokeAsync(() => {
                border.Background = value ? ColorA : ColorB;
            });
            
        }
    }
}

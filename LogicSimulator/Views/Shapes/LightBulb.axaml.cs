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
            Array.Empty<int>(),
            new int[] { 0 },
            Array.Empty<int>(),
            Array.Empty<int>()
        };

        protected override void Init() => InitializeComponent();

        /*
         * Мозги
         */

        readonly SolidColorBrush ColorA = new(Color.Parse("#00ff00")); // On
        readonly SolidColorBrush ColorB = new(Color.Parse("#1c1c1c")); // Off
        public void Brain(ref bool[] ins, ref bool[] outs) {
            var value = state = ins[0];
            Dispatcher.UIThread.InvokeAsync(() => {
                border.Background = value ? ColorA : ColorB;
            });
        }

        /*
         * Для тестирования
         */

        bool state;

        public bool GetState() => state;
    }
}

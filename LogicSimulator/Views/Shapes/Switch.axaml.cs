using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using LogicSimulator.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class Switch: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 5;

        public override int CountIns => 0;
        public override int CountOuts => 1;
        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;

        protected override void Init() {
            width = 30 * 2.5;
            height = 30 * 2.5;
            InitializeComponent();
            DataContext = this;
        }

        /*
         * Обработка размеров внутренностей
         */

        public override Point[][] PinPoints { get {
            double X = base_size + width - EllipseStrokeSize / 2;
            double Y = height / 2;
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Единственный выход
            };
        } }

        /*
         * Мозги
         */

        bool my_state = false;
        Point? press_pos;

        // Данная схема работает гораздо быстрее, чем событие Tapped ;'-} Из-за того, что не обрабатывается дополнительно DoubleTapped, что гасит второй Tapped + некоторые задержки
        private static Point GetPos(PointerEventArgs e) {
            if (e.Source is not Control src) return new();
            while ((string?) src.Tag != "scene" && src.Parent != null) src = (Control) src.Parent;
            return e.GetCurrentPoint(src).Position;
        }
        private void Press(object? sender, PointerPressedEventArgs e) {
            if (e.Source is Border) press_pos = GetPos(e);
        }
        private void Release(object? sender, PointerReleasedEventArgs e) {
            if (e.Source is not Border border) return;
            if (press_pos == null || GetPos(e).Hypot((Point) press_pos) > 5) return;
            press_pos = null;

            my_state = !my_state;
            border.Background = new SolidColorBrush(Color.Parse(my_state ? "#7d1414" : "#d32f2e"));
        }

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = my_state;

        /*
         * Кастомный экспорт
         */

        public override object Export() {
            return new Dictionary<string, object> {
                ["id"] = TypeId,
                ["pos"] = GetPos(),
                ["size"] = GetSize(),
                ["state"] = my_state
            };
        }
    }
}

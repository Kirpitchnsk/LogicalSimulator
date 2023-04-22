using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LogicSimulator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class AND_2: UserControl, IGate, INotifyPropertyChanged {
        readonly Ellipse[] pins;
        public AND_2() {
            InitializeComponent();
            DataContext = this;

            List<Ellipse> list = new();
            foreach (var logic in this.LogicalChildren[0].LogicalChildren)
                if (logic is Ellipse @ellipse) list.Add(@ellipse);
            if (list.Count != 3) throw new Exception("Чё?!"); // У этой фигуры всегда 3 пина
            pins = list.ToArray();

            joins = new JoinedItems?[pins.Length];
        }

        public UserControl GetSelf() => this;
        public Point GetPos() => new(Margin.Left, Margin.Top);
        public Size GetSize() => new(Width, Height);
        public Size GetBodySize() => new(width, height);

        public void Move(Point pos) {
            Margin = new(pos.X, pos.Y, 0, 0);
            UpdateJoins(false);
        }

        public void Resize(Size size, bool global) {
            double limit = (9 + 32) * 2;
            width = size.Width.Max(limit);
            height = size.Height.Max(limit);
            RecalcSizes();
            UpdateJoins(global);
        }

        /*
         * Обработка размеров внутренностей
         */

        private readonly double base_size = 25;
        private double width = 40 * 3; // Размеры тела, а не всего UserControl
        private double height = 40 * 3;

        public double BaseSize => base_size;
        public double BaseFraction => base_size / 40;
        public double EllipseSize => BaseFraction * 30;

        public Thickness BodyStrokeSize => new(BaseFraction * 3);
        public double EllipseStrokeSize => BaseFraction * 5;
        public double PinStrokeSize => BaseFraction * 6;

        public Thickness BodyMargin => new(base_size, 0, 0, 0);
        public double BodyWidth => width;
        public double BodyHeight => height;
        public CornerRadius BodyRadius => new(width.Min(height) / 3 + BodyStrokeSize.Top);

        public Thickness[] EllipseMargins { get {
            double X = UC_Width - EllipseSize;
            double Y = height / 2 - EllipseSize - BaseFraction;
            double Y2 = height / 2 + BaseFraction;
            double Y3 = height / 2 - EllipseSize / 2;
            return new Thickness[] {
                new(0, Y, 0, 0), // Первый вход
                new(0, Y2, 0, 0), // Второй вход
                new(X, Y3, 0, 0), // Единственный выход
            };
        } }

        public Point[][] PinPoints { get {
            double X = EllipseSize - EllipseStrokeSize / 2;
            double X2 = base_size + width - EllipseStrokeSize / 2;
            double Y = height / 2 - EllipseSize / 2 - BaseFraction;
            double Y2 = height / 2 + EllipseSize / 2 + BaseFraction;
            double Y3 = height / 2;
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Первый вход
                new Point[] { new(X, Y2), new(X + PinWidth, Y2) }, // Второй вход
                new Point[] { new(X2, Y3), new(X2 + PinWidth, Y3) }, // Единственный выход
            };
        } }

        public double UC_Width => base_size * 2 + width;
        public double UC_Height => height;

        public double FontSizze => BodyRadius.TopLeft / 1.3;

        public Thickness[] ImageMargins { get {
            double R = BodyRadius.BottomLeft;
            double num = R - R / Math.Sqrt(2);
            return new Thickness[] {
                new(0, 0, num, num), // Картинка с удалителем
                new(num, 0, 0, num), // Картинка с переместителем
            };
        } }

#pragma warning disable CS0108
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108

        void RecalcSizes() {
            // Log.Write("Size: " + width + " " + height);
            PropertyChanged?.Invoke(this, new(nameof(EllipseSize)));
            PropertyChanged?.Invoke(this, new(nameof(BodyStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(EllipseStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(PinStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(BodyMargin)));
            PropertyChanged?.Invoke(this, new(nameof(BodyWidth)));
            PropertyChanged?.Invoke(this, new(nameof(BodyHeight)));
            PropertyChanged?.Invoke(this, new(nameof(BodyRadius)));
            PropertyChanged?.Invoke(this, new(nameof(EllipseMargins)));
            PropertyChanged?.Invoke(this, new(nameof(PinPoints)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Width)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Height)));
            PropertyChanged?.Invoke(this, new(nameof(FontSizze)));
            PropertyChanged?.Invoke(this, new(nameof(ImageMargins)));
        }

        /*
         * Обработка пинов
         */

        public Distantor GetPin(Ellipse finded, Visual? ref_point) {
            int n = 0;
            foreach (var pin in pins) {
                if (pin == finded) return new(this, n, ref_point);
                n++;
            }
            return new(this, -1, ref_point);
        }

        public Point GetPinPos(int n, Visual? ref_point) {
            var pin = pins[n];
            return pin.Center(ref_point); // Смотрите Utils ;'-} Там круто сделан метод
        }

        /*
         * Обработка соединений
         */

        // readonly List<JoinedItems> joins = new();
        readonly JoinedItems?[] joins;

        public void AddJoin(JoinedItems join) {
            var dist = join.A.parent == this ? join.A : join.B;
            if (dist.parent != this) throw new Exception("Такого не бывает");
            int n = dist.num;
            joins[n]?.Delete();
            joins[n] = join;
        }

        public void RemoveJoin(JoinedItems join) {
            var dist = join.A.parent == this ? join.A : join.B;
            if (dist.parent != this) throw new Exception("Такого не бывает");
            joins[dist.num] = null;
        }

        private void UpdateJoins(bool global) {
            foreach (var join in joins)
                if (join != null && (!global || join.A.parent == this)) join.Update();
        }

        public void ClearJoins() {
            foreach (var join in joins) join?.Delete();
        }
    }
}

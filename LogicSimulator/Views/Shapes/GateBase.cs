using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LogicSimulator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public abstract class GateBase: UserControl {
        protected abstract int CountIns { get; }
        protected abstract int CountOuts { get; }
        public abstract UserControl GetSelf();
        protected abstract IGate GetSelfI { get; }
        protected abstract void Init();

        protected Ellipse[] pins;

        public GateBase() {
            Init();
            int count = CountIns + CountOuts;

            List<Ellipse> list = new();
            foreach (var logic in LogicalChildren[0].LogicalChildren)
                if (logic is Ellipse @ellipse) list.Add(@ellipse);
            if (list.Count != count) throw new Exception("Чё?!"); // У этой фигуры всегда count пинов
            pins = list.ToArray();

            joins = new JoinedItems?[count];
        }

        public void Move(Point pos) {
            Margin = new(pos.X, pos.Y, 0, 0);
            UpdateJoins(false);
        }

        public void Resize(Size size, bool global) {
            double limit = (9 + 32) * 2;
            width = size.Width.Max(limit);
            height = size.Height.Max(limit / 3 * (1.5 + 0.75 * CountIns.Max(CountOuts)));
            RecalcSizes();
            UpdateJoins(global);
        }

        public Point GetPos() => new(Margin.Left, Margin.Top);
        public Size GetSize() => new(Width, Height);
        public Size GetBodySize() => new(width, height);

        /*
         * Обработка размеров внутренностей
         */

        protected readonly double base_size = 25;
        protected double width = 30 * 3; // Размеры тела, а не всего UserControl
        protected double height = 30 * 3;

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

        public double UC_Width => base_size * 2 + width;
        public double UC_Height => height;

        public double FontSizze => BodyRadius.TopLeft / 1.3;

        public Thickness[] ImageMargins {
            get {
                double R = BodyRadius.BottomLeft;
                double num = R - R / Math.Sqrt(2);
                return new Thickness[] {
                new(0, 0, num, num), // Картинка с удалителем
                new(num, 0, 0, num), // Картинка с переместителем
            };
        } }



        public abstract Point[][] PinPoints { get; }
        public Thickness[] EllipseMargins { get {
            Point[][] pins = PinPoints;
            double R2 = EllipseSize / 2;
            double X = UC_Width - EllipseSize;
            int n = 0;
            List<Thickness> list = new();
            foreach (var pin_line in pins)
                list.Add(new(n++ < CountIns ? 0 : X, pin_line[0].Y - R2, 0, 0));
            return list.ToArray();
        } }



#pragma warning disable CS0108
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108

        protected void RecalcSizes() {
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
         * Обработка соединений
         */

        protected JoinedItems?[] joins;

        public void AddJoin(JoinedItems join) {
            if (join.A.parent == this) {
                int n = join.A.num;
                joins[n]?.Delete();
                joins[n] = join;
            }
            if (join.B.parent == this) {
                int n = join.B.num;
                joins[n]?.Delete();
                joins[n] = join;
            }
        }

        public void RemoveJoin(JoinedItems join) {
            if (join.A.parent == this) joins[join.A.num] = null;
            if (join.B.parent == this) joins[join.B.num] = null;
        }

        public void UpdateJoins(bool global) {
            foreach (var join in joins)
                if (join != null && (!global || join.A.parent == this)) join.Update();
        }

        public void ClearJoins() {
            foreach (var join in joins) join?.Delete();
        }

        /*
         * Обработка пинов
         */

        public Distantor GetPin(Ellipse finded, Visual? ref_point) {
            int n = 0;
            foreach (var pin in pins) {
                if (pin == finded) return new(GetSelfI, n, ref_point, (string?) finded.Tag ?? "");
                n++;
            }
            throw new Exception("Так не бывает");
        }

        public Point GetPinPos(int n, Visual? ref_point) {
            var pin = pins[n];
            return pin.Center(ref_point); // Смотрите Utils ;'-} Там круто сделан метод
        }
    }
}

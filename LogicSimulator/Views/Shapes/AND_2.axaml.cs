using Avalonia;
using Avalonia.Controls;

namespace LogicSimulator.Views.Shapes {
    public partial class AND_2: UserControl, IGate {
        public AND_2() {
            InitializeComponent();
            DataContext = this;
        }

        public UserControl GetSelf() => this;
        public Point GetPos() => new(Margin.Left, Margin.Top);
        public Size GetSize() => new(Width, Height);

        public void Move(Point pos) {
            Margin = new(pos.X, pos.Y, 0, 0);
        }

        public void Resize(Size size) {
            width = size.Width;
            height = size.Height;
        }

        /*
         * Обработка размеров внутренностей
         */

        private readonly double base_size = 25;
        private double width = 25 * 3; // Размеры тела, а не всего UserControl
        private double height = 25 * 3;

        public double BaseSize => base_size;
        public double BaseFraction => base_size / 40;
        public double EllipseSize => BaseFraction * 30;

        public double BodyStrokeSize => BaseFraction * 3;
        public double EllipseStrokeSize => BaseFraction * 5;
        public double PinStrokeSize => BaseFraction * 6;

        public Thickness BodyMargin => new(base_size, 0, 0, 0);
        public double BodyWidth => width;
        public double BodyHeight => height;
        public CornerRadius BodyRadius => new(BaseFraction * 43);

        public Thickness[] EllipseMargins { get {
            double X = UC_Width - EllipseSize;
            double Y = width / 2 - EllipseSize - BaseFraction;
            double Y2 = width / 2 + BaseFraction;
            double Y3 = width / 2 - EllipseSize / 2;
            return new Thickness[] {
                new(0, Y, 0, 0), // Первый вход
                new(0, Y2, 0, 0), // Второй вход
                new(X, Y3, 0, 0), // Единственный выход
            };
        } }

        public Point[][] PinPoints { get {
            double X = EllipseSize - EllipseStrokeSize / 2;
            double X2 = base_size * 4 - EllipseStrokeSize / 2;
            double Y = width / 2 - EllipseSize / 2 - BaseFraction;
            double Y2 = width / 2 + EllipseSize / 2 + BaseFraction;
            double Y3 = width / 2;
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Первый вход
                new Point[] { new(X, Y2), new(X + PinWidth, Y2) }, // Второй вход
                new Point[] { new(X2, Y3), new(X2 + PinWidth, Y3) }, // Единственный выход
            };
        } }

        public double UC_Width => base_size * 5;
        public double UC_Height => base_size * 3;

        public double FontSizze => BaseFraction * 32;
    }
}

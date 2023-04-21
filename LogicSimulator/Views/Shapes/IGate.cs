using Avalonia;
using Avalonia.Controls;

namespace LogicSimulator.Views.Shapes {
    public interface IGate {
        public UserControl GetSelf();
        public Point GetPos();
        public Size GetSize();
        public Size GetBodySize();
        public void Move(Point pos);
        public void Resize(Size size);
    }
}

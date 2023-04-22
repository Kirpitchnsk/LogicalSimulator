using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using LogicSimulator.Models;

namespace LogicSimulator.Views.Shapes {
    public interface IGate {
        public UserControl GetSelf();

        public Point GetPos();
        public Size GetSize();
        public Size GetBodySize();
        public void Move(Point pos);
        public void Resize(Size size, bool global);

        public Distantor GetPin(Ellipse finded, Visual? ref_point);
        public Point GetPinPos(int n, Visual? ref_point);

        // Чую, придётся сделать базовый класс для гейтов
        public void AddJoin(JoinedItems join);
        public void RemoveJoin(JoinedItems join);

    }
}

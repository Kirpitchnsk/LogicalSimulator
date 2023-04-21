using Avalonia.Controls;
using Avalonia;
using LogicSimulator.ViewModels;

namespace LogicSimulator.Models {
    public class Mapper {

        /*
         * Обработка мыши
         */

        public void Press(Control item, Point pos) {
            Log.Write("PointerPressed: " + item.GetType().Name + " pos: " + pos);

            Move(item, pos);
        }

        public void Move(Control item, Point pos) {
            Log.Write("PointerMoved: " + item.GetType().Name + " pos: " + pos);
        }

        public void Release(Control item, Point pos) {
            Move(item, pos);
            Log.Write("PointerReleased: " + item.GetType().Name + " pos: " + pos);
        }

        public void WheelMove(Control item, double move) {
            Log.Write("WheelMoved: " + item.GetType().Name + " delta: " + (move > 0 ? 1 : -1));
        }
    }
}

using Avalonia.Controls;
using Avalonia;
using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;
using System;
using System.Collections.Generic;

namespace LogicSimulator.Models {
    public class Mapper {
        /*
         * Выборка элементов
         */

        private int selected_item = 0;
        public int SelectedItem { get => selected_item; set => selected_item = value; }

        private static IGate CreateItem(int n) {
            return n switch {
                0 => new AND_2(),
                1 => new AND_2(),
                2 => new AND_2(),
                _ => new AND_2(),
            };
        }

        public IGate[] item_types = new IGate[] {
            CreateItem(0),
            CreateItem(1),
            CreateItem(2),
        };

        public IGate GenSelectedItem() => CreateItem(selected_item);

        /*
         * Хранилище
         */

        readonly List<IGate> items = new();
        public void AddItem(IGate item) {
            items.Add(item);
        }
        public void RemoveItem(IGate item) {
            items.Remove(item);
        }

        /*
         * Определение режима перемещения
         */

        int mode = 0;
        /*
         *    Режимы:
         * 0 - ничего не делает
         * 1 - двигаем камеру
         * 2 - двигаем элемент
         * 3 - тянем элемент
         * 4 - вышвыриваем элемент
        */

        private void CalcMode(Control item) {
            var c = (string?) item.Tag;
            mode = c switch {
                "Scene" => 1,
                "Body" => 2,
                "Resizer" => 3,
                "Deleter" => 4,
                "Pin" or _ => 0,
            };
        }

        private static UserControl? GetUC(Control item) {
            while (item.Parent != null) {
                if (item is UserControl @UC) return @UC;
                item = (Control) item.Parent;
            }
            return null;
        }
        private static IGate? GetGate(Control item) {
            var UC = GetUC(item);
            if (UC is IGate @gate) return @gate;
            return null;
        }

        /*
         * Обработка мыши
         */

        Point moved_pos;
        IGate? moved_item;
        Point item_old_pos;
        Size item_old_size;

        public bool tapped = false; // Обрабатывается после Release
        public Point tap_pos; // Обрабатывается после Release

        public void Press(Control item, Point pos) {
            // Log.Write("PointerPressed: " + item.GetType().Name + " pos: " + pos);

            CalcMode(item);
            Log.Write("new_mode: " + mode);

            moved_pos = pos;
            moved_item = GetGate(item);
            tapped = true;
            if (moved_item != null) item_old_pos = moved_item.GetPos();

            switch (mode) {
            case 3:
                if (moved_item == null) break;
                item_old_size = moved_item.GetBodySize();
                break;
            }

            Move(item, pos);
        }

        public void Move(Control item, Point pos) {
            // Log.Write("PointerMoved: " + item.GetType().Name + " pos: " + pos);

            var delta = pos - moved_pos;
            if (delta.X == 0 && delta.Y == 0) return;

            if (Math.Pow(delta.X, 2) + Math.Pow(delta.Y, 2) > 9) tapped = false;

            switch (mode) {
            case 2:
                if (moved_item == null) break;
                var new_pos = item_old_pos + delta;
                moved_item.Move(new_pos);
                break;
            case 3:
                if (moved_item == null) break;
                var new_size = item_old_size + new Size(delta.X, delta.Y);
                moved_item.Resize(new_size);
                break;
            }
        }

        public int Release(Control item, Point pos) {
            Move(item, pos);
            // Log.Write("PointerReleased: " + item.GetType().Name + " pos: " + pos);

            if (tapped) Tapped(item, pos);

            int res_mode = mode;
            mode = 0;
            return res_mode;
        }

        private void Tapped(Control item, Point pos) {
            Log.Write("Tapped: " + item.GetType().Name + " pos: " + pos);
            tap_pos = pos;

            if (mode == 4 && moved_item != null) {
                RemoveItem(moved_item);
                ((Control) moved_item).Remove();
            }
        }

        public void WheelMove(Control item, double move) {
            // Log.Write("WheelMoved: " + item.GetType().Name + " delta: " + (move > 0 ? 1 : -1));
        }
    }
}

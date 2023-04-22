using Avalonia.Controls;
using Avalonia;
using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;
using System;
using System.Collections.Generic;
using DynamicData;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.LogicalTree;

namespace LogicSimulator.Models {
    public class Mapper {
        readonly Line marker = new() { Tag = "Marker", ZIndex = 2, IsVisible = false, Stroke = Brushes.YellowGreen, StrokeThickness = 3 };
        public Line Marker { get => marker; }

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
         * 5 - тянем линию от входа (In)
         * 6 - тянем линию от выхода (Out)
         * 7 - тянем линию от узла (IO)
        */

        private void CalcMode(Control item) {
            var c = (string?) item.Tag;
            mode = c switch {
                "Scene" => 1,
                "Body" => 2,
                "Resizer" => 3,
                "Deleter" => 4,
                "In" => 5,
                "Out" => 6,
                "IO" => 7,
                "Pin" or _ => 0,
            };
        }
        private static bool IsMode(Control item, string[] mods) {
            var name = (string?) item.Tag;
            if (name == null) return false;
            return mods.IndexOf(name) != -1;
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

        Ellipse? marker_circle;
        Distantor? start_dist;
        int marker_mode;

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
            case 5 or 6 or 7:
                if (marker_circle == null) break;
                var gate = GetGate(marker_circle) ?? throw new Exception("Чё?!"); // Такого не бывает
                start_dist = gate.GetPin(marker_circle, FindCanvas());

                var circle_pos = start_dist.GetPos();
                marker.StartPoint = marker.EndPoint = circle_pos;
                marker.IsVisible = true;
                marker_mode = mode;
                break;
            }

            Move(item, pos);
        }

        public Canvas? FindCanvas() {
            foreach (var item in items) {
                var p = item.GetSelf().Parent;
                if (p is Canvas @canv) return @canv;
            }
            return null;
        }
        public void FixItem(ref Control res, Point pos, IEnumerable<ILogical> items) {
            foreach (var logic in items) {
                // if (item.IsPointerOver) { } Гениальная вещь! ;'-} Хотя не, всё равно блокируется после Press и до Release, чего я впринципе хочу избежать ;'-}
                var item = (Control) logic;
                var tb = item.TransformedBounds;
                // if (tb != null && new Rect(tb.Value.Clip.TopLeft, new Size()).Sum(item.Bounds).Contains(pos) && (string?) item.Tag != "Join") res = item; // Гениально! ;'-} НАКОНЕЦ-ТО ЗАРАБОТАЛО! (Так было в 8 лабе)
                if (tb != null && tb.Value.Bounds.TransformToAABB(tb.Value.Transform).Contains(pos) && (string?) item.Tag != "Join") res = item; // Гениально! Апгрейд прошёл успешно :D
                FixItem(ref res, pos, item.GetLogicalChildren());
            }
        }
        public void Move(Control item, Point pos) {
            // Log.Write("PointerMoved: " + item.GetType().Name + " pos: " + pos);

            if (mode == 5 || mode == 6 || mode == 7) {
                var canv = FindCanvas();
                if (canv != null) {
                    var tb = canv.TransformedBounds;
                    if (tb != null) {
                        item = new Canvas() { Tag = "Scene" };
                        var bounds = tb.Value.Bounds.TransformToAABB(tb.Value.Transform);
                        FixItem(ref item, pos + bounds.TopLeft, canv.Children);
                    }
                }
            }

            string[] mods = new[] { "In", "Out", "IO" };
            var tag = (string?) item.Tag;
            if (IsMode(item, mods) && item is Ellipse @ellipse
                && !(marker_mode == 5 && tag == "In" || marker_mode == 6 && tag == "Out")) { // То самое место, что не даёт подключить вход ко входу, либо выход к выходу

                if (marker_circle != null && marker_circle != @ellipse) { // На случай моментального перехода курсором с одного кружка на другой
                    marker_circle.Fill = new SolidColorBrush(Color.Parse("#0000"));
                    marker_circle.Stroke = Brushes.Gray;
                }
                marker_circle = @ellipse;
                @ellipse.Fill = Brushes.Lime;
                @ellipse.Stroke = Brushes.Green;
            } else if (marker_circle != null) {
                marker_circle.Fill = new SolidColorBrush(Color.Parse("#0000"));
                marker_circle.Stroke = Brushes.Gray;
                marker_circle = null;
            }

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
            case 5 or 6 or 7:
                var end_pos = marker_circle == null ? pos : marker_circle.Center(FindCanvas());
                marker.EndPoint = end_pos;
                break;
            }
        }

        public int Release(Control item, Point pos) {
            Move(item, pos);
            // Log.Write("PointerReleased: " + item.GetType().Name + " pos: " + pos);

            switch (mode) {
            case 5 or 6 or 7:
                if (start_dist == null) break;
                if (marker_circle != null) {
                    var gate = GetGate(marker_circle) ?? throw new Exception("Чё?!"); // Такого не бывает
                    var end_dist = gate.GetPin(marker_circle, FindCanvas());
                    Log.Write("Стартовый элемент: " + start_dist.parent + " (" + start_dist.GetPos() + ")");
                    Log.Write("Конечный  элемент: " + end_dist.parent   + " (" + end_dist.GetPos()   + ")");
                }
                marker.IsVisible = false;
                marker_mode = 0;
                break;
            }

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

﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LogicSimulator.Views.Shapes {
    public abstract class GateBase: UserControl {
        public int CountIns { get; private set; }
        public int CountOuts { get; private set; }
        public abstract UserControl GetSelf();
        protected abstract IGate GetSelfI { get; }
        protected abstract void Init();
        protected abstract int[][] Sides { get; }

        protected readonly Line[] line_arr;
        protected readonly Ellipse[] pins;
        protected readonly Border border;

        protected bool use_top;
        protected bool use_left;
        protected bool use_right;
        protected bool use_bottom;

        public GateBase() {
            var sides = Sides;
            use_top = sides[0].Length > 0;
            use_left = sides[1].Length > 0;
            use_right = sides[2].Length > 0;
            use_bottom = sides[3].Length > 0;
            int ins = 0, outs = 0, ios = 0;
            foreach (var side in sides)
                foreach (var type in side)
                    switch (type) {
                    case 0: ins++; break;
                    case 1: outs++; break;
                    case 2: ios++; break;
                    }
            CountIns = ins;
            CountOuts = outs + ios;

            double sizer = sides.Select(x => x.Length).Max();
            width = height = 30 * (2 + sizer / 2);
            // AvaloniaXamlLoader.Load(GetSelf()); // InitializeComponent(); Не вышло :///
            // А так от Init бы полностью отказался бы ;'-} Принцип Подскановки Лископ бы просто пылал от этого, хоть абстрактному классу и положено зависеть от потомка ;'-}
            DataContext = GetSelf();
            Init(); // :///

            var canv = (Canvas) LogicalChildren[0];
            List<Line> list = new();
            List<Ellipse> list2 = new();
            if (canv.Children[0] is not Border b) throw new Exception("Такого не бывает");
            border = b;
            border.ZIndex = 2;

            foreach (var side in sides)
                foreach (var type in side) {
                    if (type < 0) continue;

                    var newy = new Line() { Tag = "Pin", ZIndex = 1, Stroke = Brushes.Gray };
                    list.Add(newy);
                    canv.Children.Add(newy);

                    var newy2 = new Ellipse() { Tag = type == 0 ? "In" : type == 1 ? "Out" : "IO", ZIndex = 2, Stroke = Brushes.Gray, Fill = new SolidColorBrush(Color.Parse("#0000")) };
                    list2.Add(newy2);
                    canv.Children.Add(newy2);
                }
            line_arr = list.ToArray();
            pins = list2.ToArray();

            joins_in = new JoinedItems?[ins];
            joins_out = new List<JoinedItems>[outs];
            for (int i = 0; i < outs; i++) joins_out[i] = new();

            MyRecalcSizes();
        }

        /*
         * Всё о размерах и позициях самого элемента ;'-}
         */

        public void Move(Point pos, bool global = false) {
            Margin = new(pos.X - UC_Width / 2, pos.Y - UC_Height / 2, 0, 0);
            // Log.Write("Пришла позиция: " + pos + " | а вышла: " + GetPos());
            UpdateJoins(global);
        }

        public void Resize(Size size, bool global = false) {
            double limit = (9 + 32) * 2 * (base_size / 25);
            width = size.Width.Max(limit / 3 * (CountIns == 0 || CountOuts == 0 ? 2.25 : 3));
            height = size.Height.Max(limit / 3 * (1.5 + 0.75 * CountIns.Max(CountOuts)));
            RecalcSizes();
            UpdateJoins(global);
        }
        public void ChangeScale(double scale, bool global = false) {
            var fix = GetPos();
            base_size *= scale;
            width *= scale;
            height *= scale;
            Move(fix, global);
            RecalcSizes();
            UpdateJoins(global);
        }

        public Point GetPos() => new(Margin.Left + UC_Width / 2, Margin.Top + UC_Height / 2);
        public Size GetSize() => new(Width, Height);
        public Size GetBodySize() => new(width, height);

        private Point pose;
        public void SavePose() => pose = GetPos();
        public Point GetPose() => pose;
        public Rect GetBounds() => new(Margin.Left, Margin.Top, UC_Width, UC_Height);

        /*
         * Обработка размеров внутренностей
         */

        protected double base_size = 25;
        protected double width = 30 * 3; // Размеры тела, а не всего UserControl
        protected double height = 30 * 3;

        public double BaseSize => base_size;
        public double BaseFraction => base_size / 40;
        public double EllipseSize => BaseFraction * 30;

        public Thickness BodyStrokeSize => new(BaseFraction * 3);
        public double EllipseStrokeSize => BaseFraction * 5;
        public double PinStrokeSize => BaseFraction * 6;

        public Thickness BodyMargin => new(use_left ? base_size : 0, use_top ? base_size : 0, 0, 0);
        public double BodyWidth => width;
        public double BodyHeight => height;
        public CornerRadius BodyRadius => new(width.Min(height) / 3 + BodyStrokeSize.Top);

        public double UC_Width => (use_left ? base_size : 0) + width + (use_right ? base_size : 0);
        public double UC_Height => (use_top ? base_size : 0) + height + (use_bottom ? base_size : 0);

        public double FontSizze => BodyRadius.TopLeft / 1.3;

        public Thickness ImageMargins { get {
            double R = BodyRadius.BottomLeft;
            double num = R - R / Math.Sqrt(2);
            return new(0, 0, num, num); // Картинка с переместителем
            // Картинка с удалителем ... устранена ;'-}
        } }



        public Point[][] PinPoints { get {
            List<Point[]> res = new();
            int n = -1;
            double R = BodyRadius.TopLeft;
            double min = EllipseSize + BaseFraction * 2;
            double pin_start = EllipseSize - EllipseStrokeSize / 2;
            double pin_width = base_size - EllipseSize + PinStrokeSize;
            // .1.
            // .1..2.
            // .1..2..3.
            foreach (var side in Sides) {
                n++;
                double count = side.Length;
                if (count == 0) continue;

                double body_len = n == 0 || n == 3 ? height : width;
                double body_len2 = n == 0 || n == 3 ? width : height;
                double delta = n < 2 ? pin_start : (n == 2 ? (use_left ? base_size : 0) : (use_top ? base_size : 0)) + body_len - EllipseStrokeSize / 2;
                double left = R, mid = body_len2 / 2, right = body_len2 - R;
                bool overflow = count > 1 && (right - left) / count < min;
                int n2 = 0;
                foreach (int type in side) {
                    double delta2 = overflow ?
                        mid + min * (n2 - (count - 1) / 2) :
                        left + (right - left) / (count * 2) * (n2 * 2 + 1);
                    if (type >= 0) res.Add(n == 0 || n == 3 ?
                        new Point[] { new(delta2, delta), new(0, pin_width) } :
                        new Point[] { new(delta, delta2), new(pin_width, 0) }
                    );
                    n2++;
                }
            }
            return res.ToArray();
        } }

        public Thickness[] EllipseMargins { get {
            Point[][] pins = PinPoints;
            double R2 = EllipseSize / 2;
            double X = UC_Width - EllipseSize;
            int n = 0;
            List<Thickness> list = new();
            foreach (var pin_line in pins)
                list.Add(new(n++ < CountIns ? 0 : X, pin_line[0].Y - R2, 0, 0));
            return ellipse_margins = list.ToArray();
        } }

        public double ImageSize => base_size / 25 * 24;



#pragma warning disable CS0108
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108

        protected void RecalcSizes() {
            // Log.Write("Size: " + width + " " + height);
            PropertyChanged?.Invoke(this, new(nameof(BodyStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(BodyMargin)));
            PropertyChanged?.Invoke(this, new(nameof(BodyWidth)));
            PropertyChanged?.Invoke(this, new(nameof(BodyHeight)));
            PropertyChanged?.Invoke(this, new(nameof(BodyRadius)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Width)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Height)));
            PropertyChanged?.Invoke(this, new(nameof(FontSizze)));
            PropertyChanged?.Invoke(this, new(nameof(ImageMargins)));
            PropertyChanged?.Invoke(this, new(nameof(ImageSize)));

            PropertyChanged?.Invoke(this, new("ButtonSize"));
            PropertyChanged?.Invoke(this, new("InvertorSize"));
            PropertyChanged?.Invoke(this, new("InvertorStrokeSize"));
            PropertyChanged?.Invoke(this, new("InvertorMargin"));

            MyRecalcSizes();
        }

        protected void MyRecalcSizes() {
            var pin_points = PinPoints;
            var pin_stroke_size = PinStrokeSize;
            int n = 0;
            foreach (var line in line_arr) {
                // Пришлось отказать от этих параметров из-за бага авалонии, т.к. в Bounds попадает мусор,
                // т.е. весь путь, который линия проходит от начала координат своего предка НЕ помечается, как Margin,
                // из-за чего подсоединение к элементам начинается сильно глючить, видя в теге Pin вместо In XD
                // DevTools тоже обманывается, что это действительно границы линии, а не Margin :/
                var A = pin_points[n][0];
                var B = pin_points[n++][1];

                line.StrokeThickness = pin_stroke_size;
                // line.StartPoint = A;
                line.Margin = new(A.X, A.Y, 0, 0);
                line.EndPoint = B;
            }

            n = 0;
            var ellipse_margin = EllipseMargins;
            var ellipse_size = EllipseSize;
            var ellipse_stroke_size = EllipseStrokeSize;
            foreach (var pin in pins) {
                pin.Margin = ellipse_margin[n++];
                pin.Width = ellipse_size;
                pin.Height = ellipse_size;
                pin.StrokeThickness = ellipse_stroke_size;
            }
        }

        /*
         * Обработка соединений
         */

        protected JoinedItems?[] joins_in;
        protected List<JoinedItems>[] joins_out;

        public void AddJoin(JoinedItems join) {
            for (int i = 0; i < 2; i++) {
                var dist = i == 0 ? join.A : join.B;
                if (dist.parent == this) {
                    int n = dist.num;
                    if (n < CountIns) {
                        joins_in[n]?.Delete();
                        joins_in[n] = join;
                        // Log.Write("AddIn: " + n);
                    } else {
                        joins_out[n - CountIns].Add(join);
                        // Log.Write("AddOut: " + CountIns);
                    }
                }
            }
            skip_upd = false;
        }

        public void RemoveJoin(JoinedItems join) {
            for (int i = 0; i < 2; i++) {
                var dist = i == 0 ? join.A : join.B;
                if (dist.parent == this) {
                    int n = dist.num;
                    if (n < CountIns) joins_in[n] = null;
                    else joins_out[n - CountIns].Remove(join);
                }
            }
            skip_upd = false;
        }

        public void UpdateJoins(bool global) {
            foreach (var join in joins_in) join?.Update();
            if (!global)
                foreach (var joins in joins_out)
                    foreach (var join in joins) join.Update();
        }

        public void ClearJoins() {
            foreach (var join in joins_in) join?.Delete();
            foreach (var joins in joins_out)
                foreach (var join in joins.ToArray()) join.Delete();
        }

        public void SetJoinColor(int o_num, bool value) {
            var joins = joins_out[o_num];
            Dispatcher.UIThread.InvokeAsync(() => { // Ох, знакомая головная боль с андроида, где даже Toast за пределами главного потока не вызовешь :/ XD :D
                foreach(var join in joins)
                    join.line.Stroke = value ? Brushes.Lime : Brushes.DarkGray;
            });
        }

        public bool ContainsJoin(JoinedItems join) {
            foreach (var join2 in joins_in) if (join == join2) return true;
            foreach (var joins in joins_out)
                foreach (var join2 in joins) if (join == join2) return true;
            return false;
        }

        /*
         * Обработка пинов
         */

        public Distantor GetPin(Ellipse finded) {
            int n = 0;
            foreach (var pin in pins) {
                if (pin == finded) return new(GetSelfI, n, (string?) finded.Tag ?? "");
                n++;
            }
            throw new Exception("Так не бывает");
        }

        /* Внимание! TransformedBounds в принципе не обновляется, когда мне это надо, сколько бы времени
         * не прошло, ПО ЭТОМУ высчет центра окружности через TransformedBounds отстаёт!
         * По этому от метода Center, что я сделал в Utils, придётся отказаться XD
         * 
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
            base.OnPropertyChanged(change);
            if (change.Property.Name == "TransformedBounds")
                Log.Write("Что-то изменилось " + change.NewValue.Value);
            else
                Log.Write("Что-то изменилось " + change.Property.Name + " " + change.NewValue.Value);
        }*/

        Thickness[] ellipse_margins = Array.Empty<Thickness>();

        public Point GetPinPos(int n) {
            // var pin = pins[n];
            // return pin.Center(ref_point); // Смотрите Utils ;'-} Там круто сделан метод (но он по факту и оказался причиной бага, т.к. TransformedBounds ОПАЗДЫВАААААЕЕЕЕЕЕЕЕЕЕЕЕЕЕТ!)
            var m = ellipse_margins[n];
            double R2 = EllipseSize / 2;
            return new Point(Margin.Left + m.Left + R2, Margin.Top + m.Top + R2);
        }

        /*
         * Мозги
         */

        bool skip_upd = true;
        public void LogicUpdate(Dictionary<IGate, Meta> ids, Meta me) {
            if (skip_upd) return;
            skip_upd = true;

            int ins = CountIns;
            for (int i = 0; i < ins; i++) {
                var join = joins_in[i];
                if (join == null) { me.ins[i] = 0; continue; }

                if (join.A.parent == this) {
                    var item = join.B;
                    if (item.tag == "Out" || item.tag == "IO") {
                        var p = item.parent;
                        Meta meta = ids[p];
                        me.ins[i] = meta.outs[item.num - p.CountIns];
                    }
                }
                if (join.B.parent == this) {
                    var item = join.A;
                    if (item.tag == "Out" || item.tag == "IO") {
                        var p = item.parent;
                        Meta meta = ids[p];
                        me.ins[i] = meta.outs[item.num - p.CountIns];
                    }
                }
            }
        }

        /*
         * Экспорт, но может быть прокачан в дочернем классе, если есть что добавить
         */

        public abstract int TypeId { get; }

        public object Export() {
            var res = new Dictionary<string, object> {
                ["id"] = TypeId,
                ["pos"] = GetPos(),
                ["size"] = GetBodySize(),
                ["base_size"] = base_size
            };
            var res2 = ExtraExport();
            if (res2 != null) foreach (var item in res2) res.Add(item.Key, item.Value);
            return res;
        }
        public virtual Dictionary<string, object>? ExtraExport() => null;

        public List<object[]> ExportJoins(Dictionary<IGate, int> to_num) {
            List<object[]> res = new();
            foreach (var joins in joins_out) foreach (var join in joins) {
                Distantor a = join.A, b = join.B;
                res.Add(new object[] {
                    to_num[a.parent], a.num, a.tag,
                    to_num[b.parent], b.num, b.tag,
                });
            }
            return res;
        }

        public void Import(Dictionary<string, object> dict) {
            double new_b_size = base_size;
            Point new_pos = GetPos();
            Size new_size = GetSize();
            foreach (var item in dict) {
                object value = item.Value;
                switch (item.Key) {
                case "id":
                    if (value is int @id) {
                        if (@id != TypeId) throw new ArgumentException("ВНИМАНИЕ! Пришёл не верный id: " + @id + " Ожидалось: " + TypeId);
                    } else Log.Write("Неверный тип id-записи элемента: " + value);
                    break;
                case "pos":
                    if (value is Point @pos) new_pos = @pos;
                    else Log.Write("Неверный тип pos-записи элемента: " + value);
                    break;
                case "base_size":
                    double? b_size = value.ToDouble();
                    if (b_size != null) new_b_size = (double) b_size;
                    else Log.Write("Неверный тип base_size-записи элемента: " + value);
                    break;
                case "size":
                    if (value is Size @size) new_size = @size;
                    else Log.Write("Неверный тип size-записи элемента: " + value);
                    break;
                default:
                    ExtraImport(item.Key, value);
                    break;
                }
            }
            base_size = new_b_size;
            Resize(new_size);
            Move(new_pos);
        }
        public virtual void ExtraImport(string key, object extra) {
            Log.Write(key + "-запись элемента не поддерживается");
        }

        /* Для тестирования */

        public Ellipse SecretGetPin(int n) => pins[n];
    }
}

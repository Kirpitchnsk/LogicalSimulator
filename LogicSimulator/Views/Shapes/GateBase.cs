using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public abstract class GateBase: UserControl {
        public abstract int CountIns { get; }
        public abstract int CountOuts { get; }
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

            joins_in = new JoinedItems?[CountIns];
            joins_out = new List<JoinedItems>[CountOuts];
            for (int i = 0; i < CountOuts; i++) joins_out[i] = new();
        }

        /*
         * Всё о размерах и позициях самого элемента ;'-}
         */

        public void Move(Point pos, bool global = false) {
            Margin = new(pos.X - UC_Width / 2, pos.Y - UC_Height / 2, 0, 0);
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
            base_size *= scale;
            width *= scale;
            height *= scale;
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
            return ellipse_margins = list.ToArray();
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

            PropertyChanged?.Invoke(this, new("ButtonSize"));
            PropertyChanged?.Invoke(this, new("InvertorSize"));
            PropertyChanged?.Invoke(this, new("InvertorStrokeSize"));
            PropertyChanged?.Invoke(this, new("InvertorMargin"));
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

        public virtual object Export() {
            return new Dictionary<string, object> {
                ["id"] = TypeId,
                ["pos"] = GetPos(),
                ["size"] = GetBodySize(),
                ["base_size"] = base_size
            };
        }

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

        public virtual void Import(Dictionary<string, object> dict) {
            if (!@dict.TryGetValue("pos", out var @value)) { Log.Write("pos-запись элемента не обнаружен"); return; }
            if (@value is not Point @pos) { Log.Write("Неверный тип pos-записи элемента: " + @value); return; }
            Move(@pos);

            if (@dict.TryGetValue("base_size", out var @value3))
                if (@value3 is double @b_size) base_size = @b_size;

            if (!@dict.TryGetValue("size", out var @value2)) { Log.Write("size-запись элемента не обнаружен"); return; }
            if (@value2 is not Size @size) { Log.Write("Неверный тип size-записи элемента: " + @value2); return; }
            Resize(@size);
        }
    }
}

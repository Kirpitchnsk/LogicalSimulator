using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using LogicSimulator.Models;
using LogicSimulator.Views.Shapes;
using ReactiveUI;
using System.Collections.Generic;

namespace LogicSimulator.ViewModels {
    public class Log {
        static readonly List<string> logs = new();
        // static readonly string path = "../../../Log.txt";
        // static bool first = true;

        public static MainWindowViewModel? Mwvm { private get; set; }
        public static void Write(string message, bool without_update = false) {
            if (!without_update) {
                foreach (var mess in message.Split('\n')) logs.Add(mess);
                while (logs.Count > 50) logs.RemoveAt(0);

                if (Mwvm != null) Mwvm.Logg = string.Join('\n', logs);
            }

            // if (first) File.WriteAllText(path, message + "\n");
            // else File.AppendAllText(path, message + "\n");
            // first = false;
        }
    }

    public class MainWindowViewModel: ViewModelBase {
        private string log = "";
        // Canvas canv = new();
        readonly Mapper map = new();
        public string Logg { get => log; set => this.RaiseAndSetIfChanged(ref log, value); }

        public MainWindowViewModel() { // Если я буду Window mw передавать через этот конструктор, то предварительный просмотр снова порвёт смачно XD
            Log.Mwvm = this;

            /* Так не работает :/
            var app = Application.Current;
            if (app == null) return; // Такого не бывает
            var life = (IClassicDesktopStyleApplicationLifetime?) app.ApplicationLifetime;
            if (life == null) return; // Такого не бывает
            foreach (var w in life.Windows) Log.Write("Window: " + w);
            Log.Write("Windows: " + life.Windows.Count); */
        }

        public void AddWindow(Window mw) {
            var canv = mw.Find<Canvas>("Canvas");
            if (canv == null) return; // Такого не бывает
            // this.canv = canv;

            canv.Children.Add(map.Marker);

            var panel = (Panel?) canv.Parent;
            if (panel == null) return; // Такого не бывает

            panel.PointerPressed += (object? sender, PointerPressedEventArgs e) => {
                if (e.Source != null && e.Source is Control @control) map.Press(@control, e.GetCurrentPoint(canv).Position);
            };
            panel.PointerMoved += (object? sender, PointerEventArgs e) => {
                if (e.Source != null && e.Source is Control @control) map.Move(@control, e.GetCurrentPoint(canv).Position);
            };
            panel.PointerReleased += (object? sender, PointerReleasedEventArgs e) => {
                if (e.Source != null && e.Source is Control @control) {
                    int mode = map.Release(@control, e.GetCurrentPoint(canv).Position);
                    bool tap = map.tapped;
                    if (tap && mode == 1) {
                        var pos = map.tap_pos;
                        if (canv == null) return; // Такого не бывает

                        var newy = map.GenSelectedItem();
                        var size = newy.GetSize() / 2;
                        newy.Move(pos - new Point(size.Width, size.Height));
                        canv.Children.Add(newy.GetSelf());
                        map.AddItem(newy);
                    }

                    if (map.new_join != null) {
                        canv.Children.Add(map.new_join);
                        map.new_join = null;
                    }
                }
            };
            panel.PointerWheelChanged += (object? sender, PointerWheelEventArgs e) => {
                if (e.Source != null && e.Source is Control @control) map.WheelMove(@control, e.Delta.Y);
            };
        }

        public IGate[] ItemTypes { get => map.item_types; }
        public int SelectedItem { get => map.SelectedItem; set => map.SelectedItem = value; }
    }
}
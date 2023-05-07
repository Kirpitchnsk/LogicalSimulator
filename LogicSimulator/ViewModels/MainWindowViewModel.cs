using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using LogicSimulator.Models;
using LogicSimulator.Views;
using LogicSimulator.Views.Shapes;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive;

namespace LogicSimulator.ViewModels {
    public class Log {
        static readonly List<string> logs = new();
        static readonly string path = "../../../Log.txt";
        static bool first = true;

        static readonly bool use_file = false;

        public static MainWindowViewModel? Mwvm { private get; set; }
        public static void Write(string message, bool without_update = false) {
            if (!without_update) {
                foreach (var mess in message.Split('\n')) logs.Add(mess);
                while (logs.Count > 45) logs.RemoveAt(0);

                if (Mwvm != null) Mwvm.Logg = string.Join('\n', logs);
            }

            if (use_file) {
                if (first) File.WriteAllText(path, message + "\n");
                else File.AppendAllText(path, message + "\n");
                first = false;
            }
        }
    }

    public class MainWindowViewModel: ViewModelBase, INotifyPropertyChanged {
        private string log = "";
        public string Logg { get => log; set {
            // this.RaiseAndSetIfChanged(ref log, value); Почему-то сломался из-за добавления INotifyPropertyChanged
            if (log == value) return;
            log = value;
            PropertyChanged?.Invoke(this, new(nameof(Logg)));
        } }

        public MainWindowViewModel() { // Если я буду Window mw передавать через этот конструктор, то предварительный просмотр снова порвёт смачно XD
            Log.Mwvm = this;
            Comm = ReactiveCommand.Create<string, Unit>(n => { FuncComm(n); return new Unit(); });
            NewItem = ReactiveCommand.Create<Unit, Unit>(_ => { FuncNewItem(); return new Unit(); });

            /* Так не работает :/
            var app = Application.Current;
            if (app == null) return; // Такого не бывает
            var life = (IClassicDesktopStyleApplicationLifetime?) app.ApplicationLifetime;
            if (life == null) return; // Такого не бывает
            foreach (var w in life.Windows) Log.Write("Window: " + w);
            Log.Write("Windows: " + life.Windows.Count); */
        }

        private Window? mw;
        public void AddWindow(Window window) {
            var canv = window.Find<Canvas>("Canvas");

            mw = window;
            map.canv = canv;
            if (canv == null) return; // Такого не бывает

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
                        newy.Move(pos);
                        map.AddItem(newy);
                    }
                }
            };
            panel.PointerWheelChanged += (object? sender, PointerWheelEventArgs e) => {
                if (e.Source != null && e.Source is Control @control) map.WheelMove(@control, e.Delta.Y, e.GetCurrentPoint(canv).Position);
            };
        }

        public static IGate[] ItemTypes { get => map.item_types; }
        public static int SelectedItem { get => map.SelectedItem; set => map.SelectedItem = value; }

        /*
         * Обработка той самой панели со схемами проекта
         */

        Grid? cur_grid;
        TextBlock? old_b_child;
        object? old_b_child_tag;
        string? prev_scheme_name;

        public static string ProjName { get => current_proj == null ? "???" : current_proj.Name; }

        public static ObservableCollection<Scheme> Schemes { get => current_proj == null ? new() : current_proj.schemes; }



        public void DTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            var src = (Control?) e.Source;

            if (src is ContentPresenter cp && cp.Child is Border bord) src = bord;
            if (src is Border bord2 && bord2.Child is Grid g2) src = g2;
            if (src is Grid g3 && g3.Children[0] is TextBlock tb2) src = tb2;

            if (src is not TextBlock tb) return;

            var p = tb.Parent;
            if (p == null) return;

            if (old_b_child != null)
                if (cur_grid != null) cur_grid.Children[0] = old_b_child;

            if (p is not Grid g) return;
            cur_grid = g;

            old_b_child = tb;
            old_b_child_tag = tb.Tag;
            prev_scheme_name = tb.Text;

            var newy = new TextBox { Text = tb.Text }; // Изи блиц-транcформация в одну строчку ;'-}

            // Log.Write("Tag: " + tb.Tag);
            cur_grid.Children[0] = newy;
            //Log.Write("Tag: " + tb.Tag); // КААААК?!?!?!? Почему пропажа предка удаляет Tag?!

            newy.KeyUp += (object? sender, KeyEventArgs e) => {
                if (e.Key != Key.Return) return;

                if (newy.Text != prev_scheme_name) {
                    // tb.Text = newy.Text;
                    if ((string?) tb.Tag == "p_name") current_proj?.ChangeName(newy.Text);
                    else if (old_b_child_tag is Scheme scheme) scheme.ChangeName(newy.Text);
                }

                cur_grid.Children[0] = tb;
                cur_grid = null; old_b_child = null;
            };
        }

#pragma warning disable CS0108
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108
        public void Update() {
            Log.Write("Текущий проект:\n" + current_proj);

            map.ImportScheme();

            PropertyChanged?.Invoke(this, new(nameof(ProjName)));
            PropertyChanged?.Invoke(this, new(nameof(Schemes)));
        }

        /*
         * Кнопочки!
         */

        public void FuncComm(string Comm) {
            // Log.Write("Comm: " + Comm);
            switch (Comm) {
            case "Create":
                new LauncherWindow().Show();
                mw?.Hide();
                break;
            case "Open":
                new LauncherWindow().Show();
                mw?.Hide();
                break;
            case "Save":
                map.Export();
                break;
            case "Exit":
                mw?.Close();
                break;
            }
        }

        public ReactiveCommand<string, Unit> Comm { get; }

        static void FuncNewItem() {
            current_proj?.AddScheme(null);
        }

        public ReactiveCommand<Unit, Unit> NewItem { get; }
    }
}
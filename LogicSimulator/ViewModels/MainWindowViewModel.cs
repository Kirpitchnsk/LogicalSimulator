using Avalonia;
using Avalonia.Controls;
using LogicSimulator.Models;
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
        readonly Canvas canv = new();
        readonly Mapper map = new();
        public string Logg { get => log; set => this.RaiseAndSetIfChanged(ref log, value); }

        public MainWindowViewModel() {
            Log.Mwvm = this;
            Log.Write("CHECK");
            
            // canv = mw.Find<Canvas>("Canvas");
            // if (canv == null) return;
        }
    }
}
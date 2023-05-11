using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using LogicSimulator.Views;
using LogicSimulator.Views.Shapes;
using Button = Avalonia.Controls.Button;

namespace UITestsLogicSimulator {
    public class AllTestsInOnePlace {
        private readonly LauncherWindow launcherWindow = AvaloniaApp.GetMainWindow();
        private readonly MainWindow mainWindow = LauncherWindowViewModel.GetMW;
        private readonly Mapper map = ViewModelBase.map;

        private readonly Canvas canv;
        private readonly ListBox gates;



        bool first_log = true;
        readonly string path = "../../../TestLog.txt";
        private void Log(string? message) {
            message ??= "null";
            if (first_log) {
                File.WriteAllText(path, message + "\n");
                first_log = false;
            }  else File.AppendAllText(path, message + "\n");
        }

        public AllTestsInOnePlace() {
            var buttons = launcherWindow.GetVisualDescendants().OfType<Button>();
            var new_proj = buttons.First(x => (string) x.Content == "Создать новый проект");
            new_proj.Command.Execute(new_proj.CommandParameter);
            // Только таки имбовая возможность создать проект, но никогда не определять ему файл
            // сохранения, от чего данные unit-test'ы никогда не появлияют на файловую систему :D

            var vis_arr = mainWindow.GetVisualDescendants();
            canv = vis_arr.OfType<Canvas>().First(x => (string?) x.Tag == "Scene");
            // canv.PointerEnter
            // И тут выясняется, что я в принципе не могу имитировать клики по холсту, по этому
            // придётся воздействовать на приложение через Mapper на прямую.

            gates = vis_arr.OfType<ListBox>().First(x => x.Name == "Gates");
            map.sim.Stop(); // чтобы в холостую не работало, я сам задам количество тиков методом Ticks здесь
        }



        private IGate? Click(Control target, double x, double y) {
            var pos = new Point(x, y);
            map.Press(target, pos);
            int mode = map.Release(target, pos);
            Log("Tapped: " + map.tapped + " | " + mode);
            if (map.tapped && mode == 1) {
                var tpos = map.tap_pos;
                var newy = map.GenSelectedItem();
                newy.Move(tpos);
                map.AddItem(newy);
                return newy;
            }
            return null;
        }
        private void Move(Control a, Control b) {
            map.Move(a, new());
            map.Press(a, new());
            int mode = map.Release(b, new(100, 100), false); // В себе уже имеет map.Move(target, pos2)
            Log("Moved: " + map.tapped + " | " + mode);
        }
        private string Export() {
            map.Export();
            var scheme = map.current_scheme;
            if (scheme == null) return "Scheme not defined";

            scheme.Created = 123;
            scheme.Modified = 456;
            return Utils.Obj2json(scheme.Export());
        }
        private void SelectGate(int id) => gates.SelectedIndex = id; // Хоть что-то хотя бы возможно сделать чисто через визуальную часть, а не в обход обёрток, нюхающих ивенты ;'-}
        private void Ticks(int count) {
            while (count-- > 0) map.sim.TopSecretPublicTickMethod();
        }



        [Fact]
        public void GeneralTest() {
            Task.Delay(10).GetAwaiter().GetResult();

            SelectGate(0); // AND-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? gate = Click(canv, 100, 100);
            Assert.NotNull(gate);
            var data = Export();
            Assert.Equal("{\"name\": \"Newy\", \"created\": 123, \"modified\": 456, \"items\": [{\"id\": 0, \"pos\": \"$p$100,100\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [], \"states\": \"00\"}", data);

            SelectGate(1); // OR-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? gate2 = Click(canv, 150, 150);
            Assert.NotNull(gate2);

            Move(gate.SecretGetPin(2), gate2.SecretGetPin(0));

            data = Export();
            Assert.Equal("{\"name\": \"Newy\", \"created\": 123, \"modified\": 456, \"items\": [{\"id\": 0, \"pos\": \"$p$100,100\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 1, \"pos\": \"$p$150,150\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [[0, 2, \"Out\", 1, 0, \"In\"]], \"states\": \"000\"}", data);

            Log("Export: " + Export());
            Log("ОК!");
        }
    }
}
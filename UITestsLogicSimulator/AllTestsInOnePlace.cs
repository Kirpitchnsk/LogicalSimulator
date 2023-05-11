using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using LogicSimulator.Views;
using LogicSimulator.Views.Shapes;
using System.Text;
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
            new_proj.Command.Execute(null);
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
            // Log("Tapped: " + map.tapped + " | " + mode);
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
            // Log("Moved: " + map.tapped + " | " + mode);
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
        private static void SaveProject() { // Чтобы только посмотреть, что всё соединилось как надо
            var proj = ViewModelBase.TopSecretGetProj() ?? throw new Exception("А где проект? :/");
            proj.SetDir("../../..");
            proj.FileName = "tested";
            proj.Save();
        }
        private void NewScheme() {
            Export();
            var button = mainWindow.GetVisualDescendants().OfType<Button>().Last(x => x.Name == "NewScheme");
            button.Command.Execute(null);
            var button2 = mainWindow.GetVisualDescendants().OfType<Button>().Last(x => x.Name == "OpenScheme");
            button2.Command.Execute(null);
        }
        private void ImportScheme(string data) {
            object yeah = Utils.Json2obj(data) ?? new Exception("Что-то не то в JSON");
            var proj = ViewModelBase.TopSecretGetProj() ?? throw new Exception("А где проект? :/");
            Scheme clone = new(proj, yeah);
            var scheme = map.current_scheme ?? throw new Exception("А где схема? :/");
            scheme.Update(clone.items, clone.joins, clone.states);
            map.ImportScheme();
        }



        private string ComplexSolution() {
            var sim = map.sim;
            sim.ComparativeTestMode = true;

            var inputs = sim.GetSwitches();
            var outputs = sim.GetLightBulbs();
            int L = inputs.Length;
            int steps = 1 << L;

            StringBuilder sb = new();
            for (int step = 0; step < steps; step++) {
                for (int i = 0; i < L; i++) inputs[i].SetState((step & 1 << i) > 0);
                if (step > 0) sb.Append('|');
                int hits = 0;
                Ticks(1);
                while (hits++ < 1024 && sim.SomethingHasChanged) Ticks(1);
                foreach (var output in outputs) sb.Append(output.GetState() ? '1' : '0');
                sb.Append("_t" + hits);
            }
            return sb.ToString();
        }



        [Fact]
        public void GeneralTest() {
            Task.Delay(10).GetAwaiter().GetResult();

            SelectGate(0); // AND-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? gate = Click(canv, 200, 200);
            Assert.NotNull(gate);
            var data = Export();
            Assert.Equal("{\"name\": \"Newy\", \"created\": 123, \"modified\": 456, \"items\": [{\"id\": 0, \"pos\": \"$p$200,200\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [], \"states\": \"00\"}", data);

            SelectGate(3); // XOR-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? gate2 = Click(canv, 300, 300);
            Assert.NotNull(gate2);

            Move(gate.SecretGetPin(2), gate2.SecretGetPin(0)); // Соединяем gate и gate2

            data = Export();
            Assert.Equal("{\"name\": \"Newy\", \"created\": 123, \"modified\": 456, \"items\": [{\"id\": 0, \"pos\": \"$p$200,200\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 3, \"pos\": \"$p$300,300\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [[0, 2, \"Out\", 1, 0, \"In\"]], \"states\": \"000\"}", data);

            SelectGate(5); // Switch-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? button = Click(canv, 100, 150);
            IGate? button2 = Click(canv, 100, 250);
            IGate? button3 = Click(canv, 100, 350);
            Assert.NotNull(button);
            Assert.NotNull(button2);
            Assert.NotNull(button3);

            Move(button.SecretGetPin(0), gate.SecretGetPin(0));
            Move(button2.SecretGetPin(0), gate.SecretGetPin(1));
            Move(button3.SecretGetPin(0), gate2.SecretGetPin(1));

            SelectGate(7); // LightBulb-gate
            Task.Delay(1).GetAwaiter().GetResult();

            IGate? ball = Click(canv, 400, 300);
            Assert.NotNull(ball);

            Move(gate2.SecretGetPin(2), ball.SecretGetPin(0));

            var input = (Switch) button;
            var input2 = (Switch) button2;
            var input3 = (Switch) button3;
            var output = (LightBulb) ball;

            StringBuilder sb = new();
            for (int i = 0; i < 8; i++) {
                input.SetState((i & 4) > 0);
                input2.SetState((i & 2) > 0);
                input3.SetState((i & 1) > 0);
                if (i > 0) sb.Append('|');
                for (int tick = 0; tick < 5; tick++) {
                    Ticks(1);
                    sb.Append(output.GetState() ? '1' : '0');
                }
            }
            Assert.Equal("00000|00111|11000|00111|11000|00111|11011|11000", sb.ToString());

            NewScheme();
            Task.Delay(1).GetAwaiter().GetResult();

            ImportScheme("{\"name\": \"Для тестирования\", \"created\": 1683838621, \"modified\": 1683839324, \"items\": [{\"id\": 5, \"pos\": \"$p$149,242\", \"size\": \"$s$75,75\", \"base_size\": 25, \"state\": false}, {\"id\": 5, \"pos\": \"$p$153,330\", \"size\": \"$s$75,75\", \"base_size\": 25, \"state\": false}, {\"id\": 5, \"pos\": \"$p$152,414\", \"size\": \"$s$75,75\", \"base_size\": 25, \"state\": false}, {\"id\": 5, \"pos\": \"$p$149,497\", \"size\": \"$s$75,75\", \"base_size\": 25, \"state\": false}, {\"id\": 9, \"pos\": \"$p$587,328\", \"size\": \"$s$105,105\", \"base_size\": 25, \"state\": \"0.1.0.1.0.0\"}, {\"id\": 3, \"pos\": \"$p$339,236\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 3, \"pos\": \"$p$348,336\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 3, \"pos\": \"$p$352,444\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 3, \"pos\": \"$p$355,546\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 9, \"pos\": \"$p$594,460\", \"size\": \"$s$105,105\", \"base_size\": 25, \"state\": \"0.0.1.0.0.0\"}, {\"id\": 3, \"pos\": \"$p$591,182\", \"size\": \"$s$90,90\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$749,199\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$750,276\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$751,354\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$751,430\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$752,506\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 7, \"pos\": \"$p$755,584\", \"size\": \"$s$75,75\", \"base_size\": 25}, {\"id\": 1, \"pos\": \"$p$592,596\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [[0, 0, \"Out\", 5, 0, \"In\"], [1, 0, \"Out\", 6, 0, \"In\"], [2, 0, \"Out\", 7, 0, \"In\"], [3, 0, \"Out\", 8, 0, \"In\"], [4, 3, \"Out\", 6, 1, \"In\"], [11, 0, \"In\", 4, 3, \"Out\"], [4, 4, \"Out\", 10, 0, \"In\"], [12, 0, \"In\", 4, 4, \"Out\"], [13, 0, \"In\", 4, 5, \"Out\"], [4, 5, \"Out\", 17, 0, \"In\"], [4, 0, \"In\", 5, 2, \"Out\"], [6, 2, \"Out\", 4, 1, \"In\"], [6, 2, \"Out\", 9, 2, \"In\"], [7, 2, \"Out\", 4, 2, \"In\"], [7, 2, \"Out\", 9, 1, \"In\"], [8, 2, \"Out\", 9, 0, \"In\"], [9, 3, \"Out\", 10, 1, \"In\"], [14, 0, \"In\", 9, 3, \"Out\"], [15, 0, \"In\", 9, 4, \"Out\"], [9, 4, \"Out\", 17, 1, \"In\"], [16, 0, \"In\", 9, 5, \"Out\"], [9, 5, \"Out\", 7, 1, \"In\"], [10, 2, \"Out\", 5, 1, \"In\"], [17, 2, \"Out\", 8, 1, \"In\"]], \"states\": \"00000000000000000000000\"}");
            var res = ComplexSolution();
            Assert.Equal("110001_t4|001010_t10|111011_t8|011011_t5|100100_t9|011111_t10|101110_t8|001110_t5|010001_t6|110001_t5|001110_t11|111111_t8|000100_t8|100100_t5|011011_t11|101010_t8", res);

            Log("Export: " + Export());
            Log("ОК!");
            SaveProject();
        }
    }
}
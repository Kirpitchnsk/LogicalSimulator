using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DynamicData;
using HarfBuzzSharp;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using LogicSimulator.Views;
using LogicSimulator.Views.Shapes;

namespace UITestsLogicSimulator {
    public class AllTestsInOnePlace {
        private readonly LauncherWindow launcherWindow = AvaloniaApp.GetMainWindow();
        private readonly MainWindow mainWindow = LauncherWindowViewModel.GetMW;
        private readonly Mapper map = ViewModelBase.map;

        private readonly Canvas canv;



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

            canv = mainWindow.GetVisualDescendants().OfType<Canvas>().First(x => (string?) x.Tag == "Scene");
            // canv.PointerEnter
            // И тут выясняется, что я в принципе не могу имитировать клики по холсту, по этому
            // придётся воздействовать на приложение через Mapper на прямую.

            map.sim.Stop();
            // GeneralTest();
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
        private void Move(Control target, double x, double y, double x2, double y2) {
            Point pos = new(x, y), pos2 = new(x2, y2);
            map.Press(target, pos);
            int mode = map.Release(target, pos2); // В себе уже имеет map.Move(target, pos2)
            Log("Moved: " + map.tapped + " | " + mode);
        }
        private string Export() {
            map.Export();
            var scheme = map.current_scheme;
            return scheme != null ? Utils.Obj2json(scheme.Export()) : "Scheme not defined";
        }



        [Fact]
        public async void GeneralTest() {
            await Task.Delay(10);

            Click(canv, 100, 100);
            var data = Export();
            Assert.Equal("{\"name\": \"Newy\", \"created\": 1683824960, \"modified\": 1683824960, \"items\": [{\"id\": 0, \"pos\": \"$p$100,100\", \"size\": \"$s$90,90\", \"base_size\": 25}], \"joins\": [], \"states\": \"00\"}", data);

            // Assert.Equal("Есть пустые КС|1,333 1,333 0,333 1,667 1,1667|NaN|--", TextAll());
            // Assert.Equal("Yellow|Yellow|Red|Green|Yellow", AllColors());
            // Assert.Equal("А А А|2,0,1,1~В В В|1,2,0,2~И И И|1,2,0,2", SuperLoader());

        }
    }
}
using Avalonia.Controls;
using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace LogicSimulator.Models {
    public class FileHandler {
        readonly string AppData;
        readonly List<Project> projects = new();
        readonly List<string> project_paths = new();

        public FileHandler() {
            string app_data = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            app_data = Path.Combine(app_data, "LogicSimulator");
            if (!Directory.Exists(app_data)) Directory.CreateDirectory(app_data);
            AppData = app_data;
            LoadProjectList();
        }



        private static string GetProjectFileName(string dir) {
            int n = 0;
            while (true) {
                string name = "proj_" + ++n + ".json";
                if (!File.Exists(Path.Combine(dir, name))) return name;
            }
        }



        public Project CreateProject() {
            var proj = new Project(this);
            projects.Add(proj);
            return proj;
        }
        private Project? LoadProject(string dir, string fileName) {
            try {
                var obj = Utils.Xml2obj(File.ReadAllText(Path.Combine(dir, fileName))) ?? throw new DataException("Не верная структура XML-файла проекта!");
                var proj = new Project(this, dir, fileName, obj);
                projects.Add(proj);
                return proj;
            } catch (Exception e) { Log.Write("Неудачная попытка загрузить проект:\n" + e); }
            return null;
        }
        private void LoadProjectList() {
            var file = Path.Combine(AppData, "project_list.db");
            if (!File.Exists(file)) return;

            object data;
            try { data = Utils.None(File.ReadAllText(file)) ?? throw new DataException("Не верная структура SQLite (.db)-файла списка проектов!"); } catch (Exception e) { Log.Write("Неудачная попытка загрузить список проектов:\n" + e); return; }

            if (data is not List<object> @arr) { Log.Write("В списке проектов на верхнем уровне ожидалось увидеть список"); return; }
            foreach (var path in @arr) {
                if (path is not string @str) { Log.Write("Один из путей списка проектов - не строка: " + path); continue; }
                project_paths.Add(@str);

                var s_arr = @str.Split(Path.DirectorySeparatorChar).ToList();
                var name = s_arr[^1];
                s_arr.RemoveRange(s_arr.Count - 1, 1);
                var dir = Path.Combine(s_arr.ToArray());

                LoadProject(dir, name);
            }
        }



        internal static void SaveProject(Project proj) {
            var dir = proj.FileDir;
            if (dir == null) return;

            var data = Utils.Obj2json(proj.Export());
            var name = proj.FileName;
            name ??= GetProjectFileName(dir);
            proj.FileName = name;

            var path = Path.Combine(dir, name);
            File.WriteAllText(path, data);
        }
        private void SaveProjectList() {
            var file = Path.Combine(AppData, "project_list.db");
            if (Path.Exists(file)) File.WriteAllBytes(file, Array.Empty<byte>());
            Utils.Obj2sqlite_proj_list(project_paths.ToArray(), file);
        }

        internal Project[] GetSortedProjects() {
            projects.Sort();
            return projects.ToArray();
        }
        internal void AppendProject(Project proj) {
            if (proj.FileDir == null || proj.FileName == null) return;
            Log.Write("YEAH 1");

            var path = Path.Combine(proj.FileDir, proj.FileName);
            if (project_paths.Contains(path)) return;
            Log.Write("YEAH 2");

            project_paths.Add(path);
            SaveProjectList();
        }



        internal static string? RequestProjectPath(Window parent) {
            var dlg = new OpenFolderDialog {
                Title = "Выберите папку, куда надо сохранить новый проект"
            };
            var task = dlg.ShowAsync(parent);
            return task.GetAwaiter().GetResult();

            /* var dlg = new OpenFileDialog {
                Title = "Выберите файл, в который надо сохранить новый проект"
            };
            dlg.Filters?.Add(new FileDialogFilter() { Name = "JSON Files", Extensions = { "json" } });
            dlg.Filters?.Add(new FileDialogFilter() { Name = "All Files", Extensions = { "*" } });
            dlg.AllowMultiple = false;

            var task = dlg.ShowAsync(parent);
            var res = task.GetAwaiter().GetResult();
            Log.Write("res: " + res);*/
        }
    }
}

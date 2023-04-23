using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSimulator.Models {
    public class Project {
        public string Name { get; set; }
        public long Created;
        public long Modified;

        public List<Scheme> schemes = new();
        public List<string> scheme_files = new();
        public string FileName { get; }

        public Project() { // Новый проект
            Name = "Новый проект";
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            CreateScheme();
            FileName = FileHandler.GetProjectFileName();
        }

        public Project(string fileName, object data) { // Импорт
            FileName = fileName;
            Name = "?";
            Created = Modified = -1;

            if (data is not Dictionary<string, object> dict) { Log.Write("Ожидался словарь в корне проекта"); return; }
            
            if (!dict.TryGetValue("name", out var value)) { Log.Write("В проекте нет имени"); return; }
            if (value is not string name) { Log.Write("Тип имени проекта - не строка"); return; }
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) { Log.Write("В проекте нет времени создания"); return; }
            if (value2 is not int create_t) { Log.Write("Время создания проекта - не строка"); return; }
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) { Log.Write("В проекте нет времени изменения"); return; }
            if (value3 is not int mod_t) { Log.Write("Время изменения проекта - не строка"); return; }
            Modified = mod_t;

            if (!dict.TryGetValue("schemes", out var value4)) { Log.Write("В проекте нет списка схем"); return; }
            if (value4 is not string[] arr) { Log.Write("Списко схем проекта - не массив строк"); return; }
            foreach (var file in arr) scheme_files.Add(file);
        }



        public Scheme CreateScheme() {
            var scheme = new Scheme();
            schemes.Add(scheme);
            scheme.Save();
            scheme_files.Add(scheme.FileName);
            Save();
            return scheme;
        }

        bool loaded = false;
        private void LoadSchemes() {
            if (loaded) return;
            foreach (var fileName in scheme_files) schemes.Add(FileHandler.LoadScheme(fileName));
            loaded = true;
        }
        public Scheme GetFirstCheme() {
            LoadSchemes();
            return schemes[0];
        }



        public object Export() {
            return new Dictionary<string, object> {
                ["name"] = Name,
                ["created"] = Created,
                ["modified"] = Modified,
                ["schemes"] = schemes.Select(x => x.FileName).ToArray(),
            };
        }

        public void Save() => FileHandler.SaveProject(this);
    }
}

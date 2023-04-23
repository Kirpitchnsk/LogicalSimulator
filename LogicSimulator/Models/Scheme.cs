using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;

namespace LogicSimulator.Models {
    public class Scheme {
        public string Name { get; set; }
        public long Created;
        public long Modified;

        public object[] items;
        public object[] joins;
        public bool[] states;

        public string FileName { get; }

        public Scheme() { // Новая схема
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Name = "Newy";
            items = joins = Array.Empty<object>();
            states = Array.Empty<bool>();
            FileName = FileHandler.GetSchemeFileName();
        }

        public Scheme(string fileName, object data) { // Импорт
            FileName = fileName;
            Name = "?";
            Created = Modified = -1;
            items = joins = Array.Empty<object>();
            states = Array.Empty<bool>();

            if (data is not Dictionary<string, object> dict) { Log.Write("Ожидался словарь в корне схемы"); return; }

            if (!dict.TryGetValue("name", out var value)) { Log.Write("В схеме нет имени"); return; }
            if (value is not string name) { Log.Write("Тип имени схемы - не строка"); return; }
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) { Log.Write("В схеме нет времени создания"); return; }
            if (value2 is not int create_t) { Log.Write("Время создания схемы - не строка"); return; }
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) { Log.Write("В схеме нет времени изменения"); return; }
            if (value3 is not int mod_t) { Log.Write("Время изменения схемы - не строка"); return; }
            Modified = mod_t;

            if (!dict.TryGetValue("items", out var value4)) { Log.Write("В схеме нет списка элементов"); return; }
            if (value4 is not object[] arr) { Log.Write("Список элементов схемы - не массив объектов"); return; }
            items = arr;

            if (!dict.TryGetValue("joins", out var value5)) { Log.Write("В схеме нет списка соединений"); return; }
            if (value5 is not object[] arr2) { Log.Write("Список соединений схемы - не массив объектов"); return; }
            joins = arr2;

            if (!dict.TryGetValue("states", out var value6)) { Log.Write("В схеме нет списка состояний"); return; }
            if (value6 is not bool[] arr3) { Log.Write("Список состояний схемы - не массив bool"); return; }
            states = arr3;
        }

        public void Update(object[] items, object[] joins, bool[] states) {
            this.items = items;
            this.joins = joins;
            this.states = states;
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Save();
        }



        public object Export() {
            return new Dictionary<string, object> {
                ["name"] = Name,
                ["created"] = Created,
                ["modified"] = Modified,
                ["items"] = items,
                ["joins"] = joins,
                ["states"] = states,
            };
        }
        public void Save() => FileHandler.SaveScheme(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

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

            if (data is not Dictionary<string, object> dict) throw new Exception("Ожидался словарь в корне схемы");

            if (!dict.TryGetValue("name", out var value)) throw new Exception("В схеме нет имени");
            if (value is not string name) throw new Exception("Тип имени схемы - не строка");
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) throw new Exception("В схеме нет времени создания");
            if (value2 is not int create_t) throw new Exception("Время создания схемы - не строка");
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) throw new Exception("В схеме нет времени изменения");
            if (value3 is not int mod_t) throw new Exception("Время изменения схемы - не строка");
            Modified = mod_t;

            if (!dict.TryGetValue("items", out var value4)) throw new Exception("В схеме нет списка элементов");
            if (value4 is not List<object> arr) throw new Exception("Список элементов схемы - не массив объектов");
            items = arr.ToArray();

            if (!dict.TryGetValue("joins", out var value5)) throw new Exception("В схеме нет списка соединений");
            if (value5 is not List<object> arr2) throw new Exception("Список соединений схемы - не массив объектов");
            joins = arr2.ToArray();

            if (!dict.TryGetValue("states", out var value6)) throw new Exception("В схеме нет списка состояний");
            if (value6 is not List<object> arr3) throw new Exception("Список состояний схемы - не массив bool");
            states = arr3.Select(x => (bool) x).ToArray();
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

        public override string ToString() => Name;
    }
}

using System;

namespace LogicSimulator.Models {
    public class Scheme {
        public string Name { get; set; }
        public long Created;
        public long Modified;

        public object[] items;
        public object[] joins;
        public bool[] states;

        public Scheme() { // Новая схема
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Name = "Newy";
            items = joins = Array.Empty<object>();
            states = Array.Empty<bool>();
        }

        public void Save(object[] items, object[] joins, bool[] states) {
            this.items = items;
            this.joins = joins;
            this.states = states;
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}

using Avalonia;
using LogicSimulator.Views.Shapes;
using System;

namespace LogicSimulator.Models {
    public class Distantor : IComparable {
        readonly int dist;
        public readonly int num;
        public readonly double delta;
        public IGate parent;

        public Distantor(IGate parent, double dist, int n, double d) {
            this.parent = parent;
            this.dist = (int)dist;
            num = n; delta = d;
        }

        public int CompareTo(object? obj) {
            if (obj is not Distantor @R) throw new ArgumentException("Ожидался Distantor", nameof(obj));
            return dist - @R.dist;
        }

        public Point GetPos() => parent.GetPos();
    }
}

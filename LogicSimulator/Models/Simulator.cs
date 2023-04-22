﻿using LogicSimulator.ViewModels;
using LogicSimulator.Views.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogicSimulator.Models {
    public class Meta {
        public IGate? item;
        public int[] ins;
        public int[] outs;
        public bool[] i_buf;
        public bool[] o_buf;

        public Meta(IGate item, int out_id) {
            this.item = item;
            ins = Enumerable.Repeat(0, item.CountIns).ToArray();
            outs = Enumerable.Range(out_id, item.CountOuts).ToArray();
            i_buf = Enumerable.Repeat(false, item.CountIns).ToArray();
            o_buf = Enumerable.Repeat(false, item.CountOuts).ToArray();
        }

        public void Print() {
            Log.Write("Элемент: " + item + " | Ins: " + Utils.Obj2json(ins) + " | Outs: " + Utils.Obj2json(outs));
        }
    }


    public class Simulator {
        public Simulator() {
            var task = Task.Run(async () => {
                for (;;) {
                    await Task.Delay(1000);
                    try { Tick(); }
                    catch (Exception e) { Log.Write("E: " + e); }
                }
            });
        }



        List<bool> outs = new() { false };
        List<bool> outs2 = new() { false };
        readonly List<Meta> items = new();
        readonly Dictionary<IGate, Meta> ids = new();

        public void AddItem(IGate item) {
            int out_id = outs.Count;
            for (int i = 0; i < item.CountOuts; i++) {
                outs.Add(false);
                outs2.Add(false);
            }

            // int id = items.Count;
            Meta meta = new(item, out_id);
            items.Add(meta);
            ids.Add(item, meta);

            meta.Print();
        }

        public void RemoveItem(IGate item) {
            Meta meta = ids[item];
            meta.item = null;
            foreach (var i in Enumerable.Range(0, meta.outs.Length)) meta.outs[i] = 0;
        }

        private void Tick() {
            foreach (var meta in items) {
                var item = meta.item;
                if (item == null) continue;

                item.LogicUpdate(ids, meta);

                int[] i_n = meta.ins, o_n = meta.outs;
                bool[] ib = meta.i_buf, ob = meta.o_buf;

                for (int i = 0; i < ib.Length; i++) ib[i] = outs[i_n[i]];
                item.Brain(ref ib, ref ob);
                for (int i = 0; i < ob.Length; i++) outs2[o_n[i]] = ob[i];
            }

            (outs2, outs) = (outs, outs2);
            Log.Write("Выходы: " + Utils.Obj2json(outs));
        }
    }
}
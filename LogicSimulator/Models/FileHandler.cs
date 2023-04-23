using System.Collections.Generic;
using System.IO;

namespace LogicSimulator.Models {
    public class FileHandler {
        readonly string dir = "../../../../storage/";
        readonly List<string> projects = new();

        public FileHandler() {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            foreach (var name in Directory.GetFiles(dir)) {
                if (name.StartsWith("proj_")) {
                    // int n = int.Parse(name.Split("_")[1].Split(".")[0]);
                    projects.Add(name);
                }
            }
        }
    }
}

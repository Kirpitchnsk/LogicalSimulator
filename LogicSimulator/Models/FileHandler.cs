using System.Collections.Generic;
using System.Data;
using System.IO;

namespace LogicSimulator.Models {
    public class FileHandler {
        readonly static string dir = "../../../../storage/";
        readonly List<Project> projects = new();

        public FileHandler() {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            foreach (var name in Directory.GetFiles(dir))
                if (name.StartsWith("proj_")) LoadProject(name);
        }



        public static string GetProjectFileName() {
            int n = 0;
            while (true) {
                string name = "proj_" + ++n + ".yaml";
                if (!File.Exists(dir + name)) return name;
            }
        }
        public static string GetSchemeFileName() {
            int n = 0;
            while (true) {
                string name = "scheme_" + ++n + ".xml";
                if (!File.Exists(dir + name)) return name;
            }
        }



        public Project CreateProject() {
            var proj = new Project();
            projects.Add(proj);
            return proj;
        }
        private Project LoadProject(string fileName) {
            var obj = Utils.Yaml2obj(File.ReadAllText(dir + fileName)) ?? throw new DataException("Не верная структура YAML-файла проекта!");
            var proj = new Project(fileName, obj);
            projects.Add(proj);
            return proj;
        }
        public static Scheme LoadScheme(string fileName) {
            var obj = Utils.Yaml2obj(File.ReadAllText(dir + fileName)) ?? throw new DataException("Не верная структура XML-файла схемы!");
            var scheme = new Scheme(fileName, obj);
            return scheme;
        }



        public static void SaveProject(Project proj) {
            var data = Utils.Obj2yaml(proj.Export());
            File.WriteAllText(dir + proj.FileName, data);
        }
        public static void SaveScheme(Scheme scheme) {
            var data = Utils.Obj2xml(scheme.Export());
            File.WriteAllText(dir + scheme.FileName, data);
        }
    }
}

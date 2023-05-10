﻿using Avalonia.Data;
using ER_diagram_exTRACTOR;
using LogicSimulator.Models;
using ReactiveUI;
using System.Reflection;


// LogicSimulator.Program.Main(Array.Empty<string>()); Ура! Консольный режим изобретён ;'-}
// Type[] types = Assembly.GetExecutingAssembly().GetTypes();
// Type[] types = new Type[] { typeof(Mapper) };

// Вот это по нашему:
Type[] types = (Assembly.GetAssembly(typeof(Mapper)) ?? throw new Exception("Чё?!")).GetTypes();

List<object> items = new();
int n = 0;
foreach (Type type in types) {
    var name = type.FullName ?? throw new Exception("Чё?!");
    if (name.Contains('+') || !name.StartsWith("LogicSimulator.")) continue;
    Console.WriteLine("\nT: " + name);

    List<object> attrs = new(), meths = new();
    Dictionary<string, object?> data = new() {
        ["name"] = type.Name,
        ["stereo"] = 0,
        ["access"] = 1,
        ["attributes"] = attrs,
        ["methods"] = meths,
    };
    List<object> item = new() { data, 25 + 225 * (n % 7), 25 + 175 * (n / 7), 200, 150 };
    n++;
    items.Add(item);

    foreach (var mem in type.GetMembers()) {
        if (mem.Module.Name != "LogicSimulator.dll") continue;
        string mem_name = mem.Name;

        Console.WriteLine($"  {mem_name,-36} | {mem.MemberType,-12} | {mem.DeclaringType == type}");
        if (mem.DeclaringType != type) continue;

        var meth_arr = type.GetMethods();
        var ctor_arr = type.GetConstructors();

        switch (mem.MemberType) {
        case MemberTypes.Field:
            var field_info = type.GetField(mem_name) ?? throw new Exception("Чё?!");

            attrs.Add(new Dictionary<string, object?>() {
                ["name"] = mem.Name,
                ["type"] = Functions.TypeRenamer(field_info.FieldType.Name),
                ["access"] = field_info.IsPrivate ? 0 : // private
                             field_info.IsPublic ? 1 : // public
                             field_info.IsFamily ? 2 : // protected
                             field_info.IsAssembly ? 3 /* package */ : 0,
                ["readonly"] = field_info.IsInitOnly,
                ["static"] = field_info.IsStatic,
                ["stereo"] = 0, // common
                ["default"] = "", //    :/
            });
            break;
        case MemberTypes.Event:
            var event_info = type.GetEvent(mem_name) ?? throw new Exception("Чё?!");
            var e_type = event_info.EventHandlerType;

            attrs.Add(new Dictionary<string, object?>() {
                ["name"] = mem.Name,
                ["type"] = Functions.TypeRenamer(e_type != null ? e_type.Name : "???"),
                ["access"] = 1, // public
                ["readonly"] = false,
                ["static"] = false,
                ["stereo"] = 1, // event
                ["default"] = "", //    :/
            });
            break;
        case MemberTypes.Property:
            var prop_info = type.GetProperty(mem_name) ?? throw new Exception("Чё?!");
            var getter = prop_info.GetGetMethod(true);
            var setter = prop_info.GetSetMethod(true);

            attrs.Add(new Dictionary<string, object?>() {
                ["name"] = mem.Name,
                ["type"] = Functions.TypeRenamer(prop_info.PropertyType.Name),
                ["access"] = getter != null ?
                    getter.IsPrivate ? 0 : // private
                    getter.IsPublic ? 1 : // public
                    getter.IsFamily ? 2 : // protected
                    getter.IsAssembly ? 3 /* package */ : 0 :
                             setter != null ?
                    setter.IsPrivate ? 0 : // private
                    setter.IsPublic ? 1 : // public
                    setter.IsFamily ? 2 : // protected
                    setter.IsAssembly ? 3 /* package */ : 0 : 0,
                ["readonly"] = false,
                ["static"] = prop_info.IsStatic(),
                ["stereo"] = 2, // property
                ["default"] = "{" +
                        (getter != null ?
                    (getter.IsPrivate ? "private" :
                    getter.IsPublic ? "public" :
                    getter.IsFamily ? "protected" :
                    getter.IsAssembly ? "package" : "?") + " get; " : "") +
                        (setter != null ?
                    (setter.IsPrivate ? "private" :
                    setter.IsPublic ? "public" :
                    setter.IsFamily ? "protected" :
                    setter.IsAssembly ? "package" : "?") + " set; " : "") +
                "}",
            });
            break;
        case MemberTypes.Method:
            foreach (var method_info in meth_arr.Where(x => x.Name == mem_name)) {
                List<object> props = new();
                foreach (var param in method_info.GetParameters()) {
                    props.Add(new Dictionary<string, object?>() {
                        ["name"] = param.Name,
                        ["type"] = Functions.TypeRenamer(param.ParameterType.Name),
                        ["default"] = param.DefaultValue + "",
                    });
                }

                meths.Add(new Dictionary<string, object?>() {
                    ["name"] = mem.Name,
                    ["type"] = Functions.TypeRenamer(method_info.ReturnType.Name),
                    ["access"] =
                        method_info.IsPrivate ? 0 : // private
                        method_info.IsPublic ? 1 : // public
                        method_info.IsFamily ? 2 : // protected
                        method_info.IsAssembly ? 3 /* package */ : 0,
                    ["stereo"] =
                        method_info.IsStatic ? 1 :
                        method_info.IsAbstract ? 2 :
                        0, // common/virtual
                    ["props"] = props,
                });
            }
            break;
        case MemberTypes.Constructor:
            foreach (var ctor_info in ctor_arr.Where(x => x.Name == mem_name)) {
                List<object> props = new();
                foreach (var param in ctor_info.GetParameters()) {
                    props.Add(new Dictionary<string, object?>() {
                        ["name"] = param.Name,
                        ["type"] = Functions.TypeRenamer(param.ParameterType.Name),
                        ["default"] = param.DefaultValue + "",
                    });
                }

                meths.Add(new Dictionary<string, object?>() {
                    ["name"] = mem.Name,
                    ["type"] = "self",
                    ["access"] =
                        ctor_info.IsPrivate ? 0 : // private
                        ctor_info.IsPublic ? 1 : // public
                        ctor_info.IsFamily ? 2 : // protected
                        ctor_info.IsAssembly ? 3 /* package */ : 0,
                    ["stereo"] = 3, // create
                    ["props"] = props,
                });
            }
            break;
        }
    }
}

Dictionary<string, object?> res = new() {
    ["items"] = items,
    ["joins"] = new List<object>(),
};

File.WriteAllText("../../../../../lab8/DiagramEditor/Export.json", Utils.Obj2json(res));
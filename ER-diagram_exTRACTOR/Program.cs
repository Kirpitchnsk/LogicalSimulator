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
                ["default"] = "",
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
        }
    }
}

Dictionary<string, object?> res = new() {
    ["items"] = items,
    ["joins"] = new List<object>(),
};

File.WriteAllText("../../../../../lab8/DiagramEditor/Export.json", Utils.Obj2json(res));
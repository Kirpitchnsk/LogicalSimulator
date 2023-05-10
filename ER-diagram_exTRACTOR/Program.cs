using LogicSimulator.Models;
using System.Reflection;

// LogicSimulator.Program.Main(Array.Empty<string>()); Ура! Консольный режим изобретён ;'-}
// Type[] types = Assembly.GetExecutingAssembly().GetTypes();
// Type[] types = new Type[] { typeof(Mapper) };

// Вот это по нашему:
Type[] types = (Assembly.GetAssembly(typeof(Mapper)) ?? throw new Exception("Чё?!")).GetTypes();

foreach (Type type in types) {
    var name = type.FullName ?? throw new Exception("Чё?!");
    if (name.Contains('+') || !name.StartsWith("LogicSimulator.")) continue;

    Console.WriteLine("\nT: " + name);
    
    foreach (var mem in type.GetMembers()) {
        if (mem.Module.Name != "LogicSimulator.dll") continue;

        Console.WriteLine($"  {mem.Name, -36} | {mem.MemberType, -12} | {mem.DeclaringType == type}");
    }
}

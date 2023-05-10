namespace ER_diagram_exTRACTOR {
    internal class Functions {
        public static string TypeRenamer(Type? type) {
            if (type == null) return "???";

            var name = type.Name;
            var arr = name.Split('[');
            arr[0] = arr[0] switch {
                "Boolean" => "bool",
                "Byte" => "byte",
                "SByte" => "signed byte",
                "Char" => "char",
                "Int16" => "short",
                "Int32" => "int",
                "Int64" => "long",
                "Int128" => "long long",
                "Half" => "half float",
                "Single" => "float",
                "Double" => "double",
                "String" => "string",
                "Void" => "void",
                "Object" => "object",
                _ => arr[0]
            };
            string res = string.Join('[', arr);
            if (res.EndsWith('&')) res = "ref " + res[..^1];

            var gen = type.GetGenericArguments();
            if (gen.Length > 0) res = res.Split('\x60')[0] +
                "<" + string.Join(", ", gen.Select(TypeRenamer)) + ">";

            return res;
        }
    }
}

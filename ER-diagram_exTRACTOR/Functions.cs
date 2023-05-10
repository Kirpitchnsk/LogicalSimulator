namespace ER_diagram_exTRACTOR {
    internal class Functions {
        public static string TypeRenamer(string name) {
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
            return string.Join('[', arr);
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

string inputRec = string.Empty;
if (!File.Exists("input.txt")) {
    Console.Write("Enter input C# record type: ");
    inputRec = Console.ReadLine() ?? string.Empty;
}
else {
    inputRec = File.ReadAllText("input.txt");
}

try {
    Console.WriteLine("Parsing code...");
    var tree = CSharpSyntaxTree.ParseText(inputRec);
    var descendantNodes = tree.GetRoot().DescendantNodes();
    Console.WriteLine("Preparing compilation...");
    var compilation = CSharpCompilation.Create("R2CComp", syntaxTrees: [tree]);
    Console.WriteLine("Preparing semantic analysis...");
    var semanticModel = compilation.GetSemanticModel(tree);
    Console.WriteLine("Transforming, please wait...");
    var codeBuilder = new StringBuilder();

    foreach (var descendantNode in descendantNodes.OfType<RecordDeclarationSyntax>()) {
        codeBuilder.AppendLine($"#pragma warning disable IDE0003");
        codeBuilder.AppendLine($"public class {descendantNode.Identifier.Text}");
        codeBuilder.AppendLine("{");

        foreach (var child in descendantNode.ParameterList!.Parameters) {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(child);
            if (declaredSymbol is IParameterSymbol propSymbol) {
                var memberType = GetNormalizedType(propSymbol.Type);
                var memberName = declaredSymbol.Name;
                codeBuilder.AppendLine($"    public {NormalizeType(memberType)} {memberName} {{ get; init; }}");
            }
        }

        var constructorArgs = string.Join(", ", descendantNode.ParameterList.Parameters
            .Select(p => FormatParam(p)));
        codeBuilder.AppendLine();
        codeBuilder.AppendLine($"    public {descendantNode.Identifier.Text}({constructorArgs})");
        codeBuilder.AppendLine("    {");
        foreach (var prop in descendantNode.ParameterList.Parameters) {
            codeBuilder.AppendLine($"        this.{prop.Identifier.Text} = {prop.Identifier.Text};");
        }
        codeBuilder.AppendLine("    }");
        EmitEqualityContract();
        EmitToString();
        EmitPrintMembers();
        EmitEquals();
        EmitEqualsV2();
        EmitGetHashCode();
        EmitProtectedConstructor();
        EmitDeconstruct();
        EmitOperators();
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine($"#pragma warning restore IDE0003");

        string? FormatParam(ParameterSyntax param) {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(param);
            if (declaredSymbol is IParameterSymbol propSymbol) {
                var memberType = GetNormalizedType(propSymbol.Type);
                var memberName = declaredSymbol.Name;
                return $"{NormalizeType(memberType)} {memberName}";
            }
            return null;
        }

        void EmitEqualityContract() {
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("    protected virtual Type EqualityContract {");
            codeBuilder.AppendLine("        get {");
            codeBuilder.AppendLine($"            return typeof({descendantNode.Identifier.Text});");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine();
        }

        void EmitPrintMembers() {
            bool isNoLongerFirst = false;
            codeBuilder.AppendLine("    protected virtual bool PrintMembers(StringBuilder builder) {");
            codeBuilder.AppendLine("        RuntimeHelpers.EnsureSufficientExecutionStack();");
            foreach (var props in descendantNode.ParameterList!.Parameters) {
                string whatToAppend = isNoLongerFirst
                    ? $"\", {props.Identifier.Text} = \""
                    : $"\"{props.Identifier.Text} = \"";
                codeBuilder.AppendLine($"        builder.Append({whatToAppend});");
                codeBuilder.AppendLine($"        builder.Append({props.Identifier.Text});");
                isNoLongerFirst = true;
            }
            codeBuilder.AppendLine("        return true;");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine();
        }

        void EmitToString() {
            codeBuilder.AppendLine("    public override string ToString() {");
            codeBuilder.AppendLine("        var builder = new StringBuilder();");
            codeBuilder.AppendLine($"        builder.Append(\"{descendantNode.Identifier.Text}\");");
            codeBuilder.AppendLine($"        builder.Append(\" {{\");");
            codeBuilder.AppendLine($"        if (this.PrintMembers(builder)) {{");
            codeBuilder.AppendLine($"            builder.Append(' ');");
            codeBuilder.AppendLine($"        }}");
            codeBuilder.AppendLine($"        builder.Append('}}');");
            codeBuilder.AppendLine($"        return builder.ToString();");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine();
        }

        void EmitEquals() {
            codeBuilder.AppendLine($"    public virtual bool Equals({descendantNode.Identifier.Text} other) {{");
            codeBuilder.Append($"        return (object)this == other || (other is not null && this.EqualityContract == other.EqualityContract && ");
            List<(string, string)> types = [];
            foreach (var props in descendantNode.ParameterList!.Parameters) {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(props);
                if (declaredSymbol is IParameterSymbol propSymbol) {
                    var memberType = GetNormalizedType(propSymbol.Type);
                    var memberName = declaredSymbol.Name;
                    types.Add((memberType, memberName));
                }
            }
            codeBuilder.Append("\r\n            ");
            codeBuilder.Append(string.Join(" &&\r\n            ", types.Select(type => {
                return $"EqualityComparer<{NormalizeType(type.Item1)}>.Default.Equals(this.{type.Item2}, other.{type.Item2})";
            })));
            codeBuilder.AppendLine(");");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine();
        }

        void EmitEqualsV2() {
            codeBuilder.AppendLine($"    public override bool Equals(object? other) {{");
            codeBuilder.AppendLine($"        return Equals((other as {descendantNode.Identifier.Text})!);");
            codeBuilder.AppendLine($"    }}");
            codeBuilder.AppendLine();
        }

        void EmitGetHashCode() {
            codeBuilder.AppendLine($"    public override int GetHashCode() {{");
            codeBuilder.AppendLine($"        var hashCode = new HashCode();");
            List<(string, string)> types = [];
            foreach (var props in descendantNode.ParameterList!.Parameters) {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(props);
                if (declaredSymbol is IParameterSymbol propSymbol) {
                    var memberType = propSymbol.Type.Name;
                    var memberName = declaredSymbol.Name;
                    types.Add((memberType, memberName));
                }
            }
            foreach (var type in types) {
                codeBuilder.AppendLine($"        hashCode.Add(this.{type.Item2});");
            }
            codeBuilder.AppendLine($"        return hashCode.ToHashCode();");
            codeBuilder.AppendLine($"    }}");
            codeBuilder.AppendLine();
        }

        void EmitProtectedConstructor() {
            codeBuilder.AppendLine($"    protected {descendantNode.Identifier.Text}({descendantNode.Identifier.Text} original) {{");
            List<(string, string)> types = [];
            foreach (var props in descendantNode.ParameterList!.Parameters) {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(props);
                if (declaredSymbol is IParameterSymbol propSymbol) {
                    var memberType = propSymbol.Type.Name;
                    var memberName = declaredSymbol.Name;
                    types.Add((memberType, memberName));
                }
            }
            foreach (var type in types) {
                codeBuilder.AppendLine($"        this.{type.Item2} = original.{type.Item2};");
            }
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine();
        }

        void EmitDeconstruct() {
            List<(string, string)> types = [];
            foreach (var props in descendantNode.ParameterList!.Parameters) {
                var declaredSymbol = semanticModel.GetDeclaredSymbol(props);
                if (declaredSymbol is IParameterSymbol propSymbol) {
                    var memberType = GetNormalizedType(propSymbol.Type);
                    var memberName = declaredSymbol.Name;
                    types.Add((memberType, memberName));
                }
            }
            string argList = string.Join(", ", types.Select(type => {
                return $"out {NormalizeType(type.Item1)} {type.Item2}";
            }));
            codeBuilder.AppendLine($"    public void Deconstruct({argList}) {{");
            foreach (var type in types) {
                codeBuilder.AppendLine($"        {type.Item2} = this.{type.Item2};");
            }
            codeBuilder.AppendLine($"    }}");
            codeBuilder.AppendLine();
        }

        void EmitOperators() {
            codeBuilder.AppendLine($"    public static bool operator ==({descendantNode.Identifier.Text} left, {descendantNode.Identifier.Text} right) {{");
            codeBuilder.AppendLine($"        return (object)left == right && left is not null && left.Equals(right);");
            codeBuilder.AppendLine($"    }}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"    public static bool operator !=({descendantNode.Identifier.Text} left, {descendantNode.Identifier.Text} right) {{");
            codeBuilder.AppendLine($"        return !(left == right);");
            codeBuilder.AppendLine($"    }}");
        }
    }

    Console.WriteLine(codeBuilder.ToString());
} catch (Exception ex) {
    Console.WriteLine("Error!");
    Console.WriteLine(ex);
}

static string NormalizeType(string input) {
    return input switch {
        "String" => "string",
        "Int8" => "sbyte",
        "Int16" => "short",
        "Int32" => "int",
        "Int64" => "long",
        "UInt8" => "byte",
        "UInt16" => "ushort",
        "UInt32" => "uint",
        "UInt64" => "ulong",
        "Object" => "object",
        "Char" => "char",
        "Single" => "float",
        "Double" => "double",
        "Boolean" => "bool",
        _ => input
    };
}

static string GetNormalizedType(ITypeSymbol typeSymbol) {
    if (typeSymbol is IArrayTypeSymbol arrayType) {
        var elementType = GetNormalizedType(arrayType.ElementType);
        return $"{elementType}[]";
    }
    return typeSymbol.Name;
}

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EthicalLab.SyntaxCheck
{
    /// <summary>
    /// Recorre Assets/**/*.cs con Roslyn (C# 9, el nivel de Unity 6) y falla si hay errores de sintaxis.
    /// No resuelve tipos (no hay UnityEngine aquí); para eso están EthicalLab.Pure y el editor de Unity.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            string assets = Path.Combine(root, "Assets");
            if (!Directory.Exists(assets))
            {
                Console.Error.WriteLine("No existe " + assets);
                return 2;
            }

            var options = new CSharpParseOptions(LanguageVersion.CSharp9, DocumentationMode.None, SourceCodeKind.Regular,
                new[] { "UNITY_EDITOR", "UNITY_6000_0_OR_NEWER", "ENABLE_INPUT_SYSTEM", "ENABLE_LEGACY_INPUT_MANAGER" });

            var files = Directory.EnumerateFiles(assets, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "Library" + Path.DirectorySeparatorChar))
                .OrderBy(f => f)
                .ToList();

            int errors = 0;
            foreach (var file in files)
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), options, file);
                foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    errors++;
                    var span = diagnostic.Location.GetLineSpan();
                    Console.WriteLine(Path.GetRelativePath(root, file) + ":" + (span.StartLinePosition.Line + 1) + ":" + (span.StartLinePosition.Character + 1) + ": " + diagnostic.Id + " " + diagnostic.GetMessage());
                }
            }

            Console.WriteLine(errors == 0
                ? "OK syntax: " + files.Count + " archivos C# en Assets/ sin errores de sintaxis (C# 9)"
                : errors + " errores de sintaxis en Assets/");
            return errors == 0 ? 0 : 1;
        }
    }
}

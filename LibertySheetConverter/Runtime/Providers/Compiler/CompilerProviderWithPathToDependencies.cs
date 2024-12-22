using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LibertySheetConverter.Runtime.Providers.Compiler
{
    public class CompilerProviderWithPathToDependencies : ICompilerProvider
    {
        private readonly ConverterEngineContext _context;
        private const string AuroraDataContainerPath = "AuroraDataContainer.dll";

        public CompilerProviderWithPathToDependencies(ConverterEngineContext context)
        {
            _context = context;
        }

        public CompileResultData Compile(Dictionary<string, string> codeLibrary)
        {
            var syntaxTrees = GetSyntaxTrees(codeLibrary);
            var references = GetMetadataReferences();
            
            references.Add(MetadataReference.CreateFromFile(GetLibraryFullPath(AuroraDataContainerPath)));
            
            foreach (var libPath in _context.SettingData.VarsData.CustomLibsPath)
            {
                references.Add(MetadataReference.CreateFromFile(libPath));
            }

            var compilation = CSharpCompilation.Create(
                _context.SettingData.VarsData.ConfigurationAssemblyName,
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, platform: Platform.AnyCpu)
            );

            SaveLibrary(compilation);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                return new CompileResultData(assembly, true);
            }
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
            }

            return new CompileResultData(null, false);
        }

        private List<SyntaxTree> GetSyntaxTrees(Dictionary<string, string> codeLibrary)
        {
            var syntaxTrees = new List<SyntaxTree> { GetVersionSyntaxTree(), GeConfigurationTypesSyntaxTree() };
            foreach (var codePair in codeLibrary)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(codePair.Value));
            }

            return syntaxTrees;
        }

        private List<PortableExecutableReference> GetMetadataReferences()
        {
            var netstandardPath = _context.SettingData.VarsData.DependenciesPath;
                /*Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "dotnet",
                "packs",
                "NETStandard.Library.Ref",
                "2.1.0",
                "ref",
                "netstandard2.1");*/
            
            return Directory.GetFiles(netstandardPath, "*.dll")
                .Select(file => MetadataReference.CreateFromFile(file))
                .ToList();
        }

        private string GetLibraryFullPath(string libraryName)
        {
            return Path.Combine(_context.SettingData.BaseDirectory, libraryName);
        }

        private SyntaxTree GetVersionSyntaxTree()
        {
            var currentDate = DateTimeOffset.Now;
            var version = $"1.{currentDate.Year}.{currentDate.Month}.{currentDate.Day}";
            var versionInfo = $@"
using System.Reflection;
[assembly: AssemblyVersion(""{version}"")]
[assembly: AssemblyFileVersion(""{version}"")]
";
            return CSharpSyntaxTree.ParseText(versionInfo);
        }
        
        private SyntaxTree GeConfigurationTypesSyntaxTree()
        {
            var code = new StringWriter();
            code.WriteLine("using System;");
            
            code.WriteLine("");
            code.WriteLine($"namespace {_context.SettingData.VarsData.MainNameSpace}");
            code.WriteLine("{");
            code.WriteLine("    public enum ConfigurationTypes");
            code.WriteLine("    {");

            foreach (var configurationName in _context.RuntimeData.ConfigurationsDataContainer.ConfigurationNames)
            {
                code.WriteLine($"        {configurationName},");
            }

            code.WriteLine("    }");
            code.WriteLine("}");
            
            return CSharpSyntaxTree.ParseText(code.ToString());
        }

        private void SaveLibrary(CSharpCompilation compilation)
        {
            var outputPath = Path.Combine(
                _context.SettingData.BaseDirectory,
                _context.SettingData.VarsData.LibsSavePath,
                _context.SettingData.VarsData.ConfigurationAssemblyName + _context.SettingData.VarsData.LibsExtension
            );

            var emitResult = compilation.Emit(outputPath);
            if (emitResult.Success)
            {
                _context.Logger?.Log($"Compilation successful! DLL saved at {outputPath}");
            }
            else
            {
                _context.Logger?.Log("Compilation failed:");
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    _context.Logger?.Log(diagnostic.ToString());
                }
            }
        }
    }
}
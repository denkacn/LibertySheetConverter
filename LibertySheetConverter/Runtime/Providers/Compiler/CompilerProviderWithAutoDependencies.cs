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
    public class CompilerProviderWithAutoDependencies : ICompilerProvider
    {
        private readonly ConverterEngineContext _context;
        private const string AuroraDataContainerPath = "AuroraDataContainer.dll";

        public CompilerProviderWithAutoDependencies(ConverterEngineContext context)
        {
            _context = context;
        }

        public CompileResultData Compile(Dictionary<string, string> codeLibrary)
        {
            var syntaxTrees = GetSyntaxTrees(codeLibrary);
            var references = GetMetadataReferences();
            
            TryAddLibraryWithDependencies(GetLibraryFullPath(AuroraDataContainerPath), references);
            
            foreach (var libPath in _context.SettingData.VarsData.CustomLibsPath)
            {
                TryAddLibraryWithDependencies(libPath, references);
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
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
        }

        private string GetLibraryFullPath(string libraryName)
        {
            return Path.Combine(_context.SettingData.BaseDirectory, libraryName);
        }

        private void TryAddLibraryWithDependencies(string libraryPath, List<PortableExecutableReference> references)
        {
            if (!File.Exists(libraryPath))
            {
                _context.Logger?.Log($"Library not found at the specified path: {libraryPath}");
                return;
            }

            AddAssemblyAndDependencies(libraryPath, references);
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

        private void AddAssemblyAndDependencies(string assemblyPath, List<PortableExecutableReference> references)
        {
            if (references.Any(r => r.Display == assemblyPath))
            {
                return;
            }

            references.Add(MetadataReference.CreateFromFile(assemblyPath));

            var assembly = Assembly.LoadFrom(assemblyPath);
            var dependencies = assembly.GetReferencedAssemblies();
            
            foreach (var dependency in dependencies)
            {
                try
                {
                    var dependencyAssembly = Assembly.Load(dependency);
                    var dependencyPath = dependencyAssembly.Location;
                    AddAssemblyAndDependencies(dependencyPath, references);
                }
                catch (Exception ex)
                {
                    _context.Logger?.Log($"Failed to load dependency {dependency.FullName}: {ex.Message}");
                }
            }
        }
    }
}
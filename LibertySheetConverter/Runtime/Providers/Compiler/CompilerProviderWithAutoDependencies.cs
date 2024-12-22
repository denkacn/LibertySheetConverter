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
    public class CompilerProviderWithAutoDependencies : CompilerProviderBase
    {
        //private const string AuroraDataContainerPath = "AuroraDataContainer.dll";

        public CompilerProviderWithAutoDependencies(ConverterEngineContext context) : base(context){}

        public override CompileResultData Compile(Dictionary<string, string> codeLibrary)
        {
            var syntaxTrees = GetSyntaxTrees(codeLibrary);
            var references = GetMetadataReferences();
            
            //TryAddLibraryWithDependencies(GetLibraryFullPath(AuroraDataContainerPath), references);
            
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
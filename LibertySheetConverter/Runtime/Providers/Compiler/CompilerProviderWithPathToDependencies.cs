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
    public class CompilerProviderWithPathToDependencies : CompilerProviderBase
    {
        //private const string AuroraDataContainerPath = "AuroraDataContainer.dll";

        public CompilerProviderWithPathToDependencies(ConverterEngineContext context) : base(context){}

        public override CompileResultData Compile(Dictionary<string, string> codeLibrary)
        {
            var syntaxTrees = GetSyntaxTrees(codeLibrary);
            var references = GetMetadataReferences();
            
            //references.Add(MetadataReference.CreateFromFile(GetLibraryFullPath(AuroraDataContainerPath)));
            
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
            
            return Directory.GetFiles(netstandardPath, "*.dll")
                .Select(file => MetadataReference.CreateFromFile(file))
                .ToList();
        }

        private string GetLibraryFullPath(string libraryName)
        {
            return Path.Combine(_context.SettingData.BaseDirectory, libraryName);
        }
    }
}
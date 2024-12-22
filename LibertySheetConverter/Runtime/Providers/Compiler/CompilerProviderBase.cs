using System;
using System.Collections.Generic;
using System.IO;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LibertySheetConverter.Runtime.Providers.Compiler
{
    public abstract class CompilerProviderBase : ICompilerProvider
    {
        protected readonly ConverterEngineContext _context;

        protected CompilerProviderBase(ConverterEngineContext context)
        {
            _context = context;
        }

        public abstract CompileResultData Compile(Dictionary<string, string> codeLibrary);
        
        protected SyntaxTree GetVersionSyntaxTree()
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
        
        protected SyntaxTree GeConfigurationTypesSyntaxTree()
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
        
        protected void SaveLibrary(CSharpCompilation compilation)
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
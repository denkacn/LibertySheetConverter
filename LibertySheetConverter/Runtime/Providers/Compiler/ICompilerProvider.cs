using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models.Results;

namespace LibertySheetConverter.Runtime.Providers.Compiler
{
    public interface ICompilerProvider
    {
        CompileResultData Compile(Dictionary<string, string> codeLibrary);
    }
}
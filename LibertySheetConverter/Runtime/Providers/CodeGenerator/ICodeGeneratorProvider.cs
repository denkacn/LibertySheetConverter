using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.CodeGenerator
{
    public interface ICodeGeneratorProvider
    {
        string Generate(string className, List<FieldData> fields);
    }
}
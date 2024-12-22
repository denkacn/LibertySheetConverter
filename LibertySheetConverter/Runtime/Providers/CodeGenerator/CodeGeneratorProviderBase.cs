using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.CodeGenerator
{
    public abstract class CodeGeneratorProviderBase : ICodeGeneratorProvider
    {
        protected readonly ConverterEngineContext _context;

        protected CodeGeneratorProviderBase(ConverterEngineContext context)
        {
            _context = context;
        }


        public abstract string Generate(string className, List<FieldData> fields);
        
        protected string GetCustomNamespaceData(string className)
        {
            var customConfigurationData = _context.RuntimeData.CustomConfigurationsData.Find(c => c.ConfigurationName == className);
            return customConfigurationData != null ? customConfigurationData.Data : "";
        }
    }
}
using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models.DataContainer;
using LibertySheetConverter.Runtime.Models.Results;

namespace LibertySheetConverter.Runtime.Models
{
    public class ConverterEngineRuntimeData
    {
        public ConfigurationsDataContainer ConfigurationsDataContainer { get; set; }
        public List<FilledConfigurationData> FilledConfigurationsData { get; set; } = new List<FilledConfigurationData>();
        public Dictionary<string, string> GeneratedCode { get; set; } = new Dictionary<string, string>();
        public CompileResultData CompileResultData { get; set; }
        public List<CustomConfigurationData> CustomConfigurationsData { get; set; } = new List<CustomConfigurationData>();
    }
}
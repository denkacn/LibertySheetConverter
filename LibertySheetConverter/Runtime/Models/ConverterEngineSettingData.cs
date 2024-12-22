using System.Collections.Generic;
using LibertySheetConverter.Runtime.Interfaces;

namespace LibertySheetConverter.Runtime.Models
{
    public class ConverterEngineSettingData
    {
        public string BaseDirectory { get; private set; }
        public ConverterEngineVarsData VarsData { get; private set; }
        public List<ICustomDataConverter> CustomValueDataParsers { get; private set; } = new List<ICustomDataConverter>();
        
        public ConverterEngineSettingData(ConverterEngineVarsData varsData, string baseDirectory)
        {
            VarsData = varsData;
            BaseDirectory = baseDirectory;
        }
        
        public void AddCustomValueDataParser(ICustomDataConverter customDataConverter)
        {
            CustomValueDataParsers.Add(customDataConverter);
        }
    }
}
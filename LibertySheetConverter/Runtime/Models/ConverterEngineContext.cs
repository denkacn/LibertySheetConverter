using LibertySheetConverter.Runtime.Interfaces;

namespace LibertySheetConverter.Runtime.Models
{
    public class ConverterEngineContext
    {
        public IConverterEngineLogger? Logger { get; private set; }
        public ConverterEngineSettingData SettingData { get; private set; }
        public ConverterEngineRuntimeData RuntimeData { get; private set; }
        
        public ConverterEngineContext(ConverterEngineSettingData settingData, IConverterEngineLogger? logger)
        {
            SettingData = settingData;
            Logger = logger;
            RuntimeData = new ConverterEngineRuntimeData();
        }
    }
}
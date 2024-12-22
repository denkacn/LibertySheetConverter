namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class ConfigurationsData
    {
        public string ConfigurationName { get; private set; }
        public FillDataContainer DataContainer { get; private set; }

        public ConfigurationsData(string configurationName, FillDataContainer dataContainer)
        {
            ConfigurationName = configurationName;
            DataContainer = dataContainer;
        }
    }
}
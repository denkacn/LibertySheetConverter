namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class CustomConfigurationData
    {
        public string ConfigurationName { get; private set; }
        public string Data { get; private set; }

        public CustomConfigurationData(string configurationName, string data)
        {
            ConfigurationName = configurationName;
            Data = data;
        }
    }
}
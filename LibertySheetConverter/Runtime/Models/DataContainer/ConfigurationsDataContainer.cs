using System.Collections.Generic;

namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class ConfigurationsDataContainer
    {
        public readonly List<ConfigurationsData> Configurations = new List<ConfigurationsData>();
        public readonly List<string> ConfigurationNames = new List<string>();
        
        public void Add(string configurationName, FillDataContainer dataContainer)
        {
            Configurations.Add(new ConfigurationsData(configurationName, dataContainer));
        }
    }
}
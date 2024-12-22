using System.Collections.Generic;

namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class FilledConfigurationData
    {
        public string ConfigurationName { get; private set; }
        public List<object> FilledData { get; private set; }

        public FilledConfigurationData(string configurationName, List<object> filledData)
        {
            ConfigurationName = configurationName;
            FilledData = filledData;
        }
    }
}
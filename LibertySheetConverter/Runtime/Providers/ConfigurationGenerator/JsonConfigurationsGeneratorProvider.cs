using LibertySheetConverter.Runtime.Models.DataContainer;
using Newtonsoft.Json;

namespace LibertySheetConverter.Runtime.Providers.ConfigurationGenerator
{
    public class JsonConfigurationsGeneratorProvider : IConfigurationsGeneratorProvider
    {
        public string GenerateConfigurationString(FilledConfigurationData filledConfiguration)
        {
            return JsonConvert.SerializeObject(filledConfiguration.FilledData.Count == 1
                ? filledConfiguration.FilledData[0]
                : filledConfiguration.FilledData);
        }
    }
}
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.ConfigurationGenerator
{
    public interface IConfigurationsGeneratorProvider
    {
        string GenerateConfigurationString(FilledConfigurationData filledConfiguration);
    }
}
using System.Threading.Tasks;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Providers.ConfigurationGenerator;
using LibertySheetConverter.Runtime.Providers.Storage;

namespace LibertySheetConverter
{
    public interface IConverterEngine
    {
        ConverterEngineContext Context { get; }
        Task<bool> Convert();
        void SetStorageProvider(IStorageProvider storageProvider);
        void SetConfigurationsGeneratorProvider(IConfigurationsGeneratorProvider configurationsGeneratorProvider);
    }
}
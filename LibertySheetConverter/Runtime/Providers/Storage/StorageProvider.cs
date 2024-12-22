using System.IO;
using System.Threading.Tasks;
using LibertySheetConverter.Runtime.Models;

namespace LibertySheetConverter.Runtime.Providers.Storage
{
    public class StorageProvider : IStorageProvider
    {
        private readonly ConverterEngineContext _context;

        public StorageProvider(ConverterEngineContext context)
        {
            _context = context;
        }

        public async Task Save(string path, string className, string extension, string saveData)
        {
            var pathToSave = $@"{_context.SettingData.BaseDirectory}\{path}\{className}{extension}";
            await File.WriteAllTextAsync(pathToSave, saveData);
            
            _context.Logger?.Log($"[StorageProvider] Save path: {pathToSave}");
        }
    }
}
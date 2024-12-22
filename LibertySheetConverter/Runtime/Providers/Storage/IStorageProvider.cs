using System.Threading.Tasks;

namespace LibertySheetConverter.Runtime.Providers.Storage
{
    public interface IStorageProvider
    {
        Task Save(string path, string className, string extension, string saveData);
    }
}
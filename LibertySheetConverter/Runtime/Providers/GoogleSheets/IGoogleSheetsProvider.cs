using System.Threading.Tasks;

namespace LibertySheetConverter.Runtime.Providers.GoogleSheets
{
    public interface IGoogleSheetsProvider
    {
        Task<bool> PreparingData();
    }
}
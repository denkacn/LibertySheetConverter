using System.Collections.Generic;

namespace LibertySheetConverter.Runtime.Interfaces
{
    public interface IDataConverter
    {
        object Convert(string value, string type);
        List<T> ConvertList<T>(string value);
    }
}
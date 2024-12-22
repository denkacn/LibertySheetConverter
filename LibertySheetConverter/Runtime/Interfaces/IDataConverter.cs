namespace LibertySheetConverter.Runtime.Interfaces
{
    public interface IDataConverter
    {
        object Convert(string value, string type);
    }
}
namespace LibertySheetConverter.Runtime.Interfaces
{
    public interface ICustomDataConverter
    {
        bool TryConvert(string value, string type, out object convertedObject);
    }
}
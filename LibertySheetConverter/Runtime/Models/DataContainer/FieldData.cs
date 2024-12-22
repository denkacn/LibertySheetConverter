namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class FieldData
    {
        public string FieldName { get; private set; }
        public string FieldType { get; private set; }
    
        public FieldData(string fieldName, string fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }
    }
}
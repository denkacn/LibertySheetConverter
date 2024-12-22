using System.Collections.Generic;

namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class FillDataContainer
    {
        public List<FieldData> Fields { get; private set; }
        public List<FillDataValues> Values { get; private set; }

        public FillDataContainer(List<FieldData> fields, List<FillDataValues> values)
        {
            Fields = fields;
            Values = values;
        }
    }
}
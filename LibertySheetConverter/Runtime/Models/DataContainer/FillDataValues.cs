using System.Collections.Generic;

namespace LibertySheetConverter.Runtime.Models.DataContainer
{
    public class FillDataValues
    {
        public readonly List<object> Values = new List<object>();
        
        public void Add(object value)
        {
            Values.Add(value);
        }
    }
}
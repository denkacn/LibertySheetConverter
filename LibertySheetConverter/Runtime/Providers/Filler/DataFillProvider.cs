using System;
using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.Filler
{
    public class DataFillProvider : IDataFillProvider
    {
        public List<object> Fill(Type classType, FillDataContainer fillDataContainer)
        {
            var result = new List<object>();
            
            foreach (var dataValue in fillDataContainer.Values)
            {
                var instance = Activator.CreateInstance(classType);
                
                for (var index = 0; index < dataValue.Values.Count; index++)
                {
                    var fieldName = fillDataContainer.Fields[index].FieldName;
                    var value = dataValue.Values[index];
                    
                    instance?.GetType().GetProperty(fieldName)?.SetValue(instance, value);
                }

                if (instance != null) result.Add(instance);
            }
            
            return result;
        }
    }
}
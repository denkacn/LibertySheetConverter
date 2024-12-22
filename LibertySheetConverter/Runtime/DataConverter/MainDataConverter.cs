using System.Collections.Generic;
using System.Globalization;
using LibertySheetConverter.Runtime.Interfaces;
using Newtonsoft.Json;

namespace LibertySheetConverter.Runtime.DataConverter
{
    public class MainDataConverter : IDataConverter
    {
        private readonly List<ICustomDataConverter> _customDataConverters = new List<ICustomDataConverter>();

        public MainDataConverter(List<ICustomDataConverter> customDataParsers)
        {
            _customDataConverters.AddRange(customDataParsers);
        }

        public object Convert(string value, string type)
        {
            if (type == "string") return value;
            else if (type == "int") return int.Parse(value);
            else if (type == "bool") return bool.Parse(value);
            else if (type == "double") return double.Parse(value, CultureInfo.InvariantCulture);
            else if (type == "float") return float.Parse(value, CultureInfo.InvariantCulture);
            else if (type == "decimal") return decimal.Parse(value, CultureInfo.InvariantCulture);
            else if (type == "char") return char.Parse(value);
            else if (type == "byte") return byte.Parse(value);
            else if (type == "short") return short.Parse(value);
            else if (type == "long") return long.Parse(value);

            else if (type == "List<int>") return ConvertList<int>(value);
            else if (type == "List<string>") return ConvertList<string>(value);
            else if (type == "List<bool>") return ConvertList<bool>(value);
            else if (type == "List<float>") return ConvertList<float>(value);

            /*else if(type == "SquadType") return (SquadType)Enum.Parse(typeof(SquadType), value);
            else if(type == "SquadSubType") return (SquadSubType)Enum.Parse(typeof(SquadSubType), value);
            else if(type == "SquadVarietyType") return (SquadVarietyType)Enum.Parse(typeof(SquadVarietyType), value);*/

            else if (TryConvertByCustomDataParser(value, type, out var parseObject)) return parseObject;

            else return value;
        }

        private bool TryConvertByCustomDataParser(string value, string type, out object convertedObject)
        {
            foreach (var customDataParser in _customDataConverters)
            {
                if (customDataParser.TryConvert(value, type, out var customConvertedObject))
                {
                    convertedObject = customConvertedObject;
                    return true;
                } 
            }
            
            convertedObject = null;
            return false;
        }

        private List<T> ConvertList<T>(string value)
        {
            return JsonConvert.DeserializeObject<List<T>>(value);
        }
    }
}
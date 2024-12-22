using System.Text.RegularExpressions;

namespace LibertySheetConverter.Runtime.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = Regex.Replace(input, @"(?<!^)([A-Z])", "_$1");
            return result.ToLower(); 
        }
    }

}
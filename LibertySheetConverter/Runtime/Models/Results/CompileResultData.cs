using System.Reflection;

namespace LibertySheetConverter.Runtime.Models.Results
{
    public class CompileResultData
    {
        public Assembly Assembly { get; private set; }
        public bool IsSuccess { get; private set; }

        public CompileResultData(Assembly assembly, bool isSuccess)
        {
            Assembly = assembly;
            IsSuccess = isSuccess;
        }
    }
}
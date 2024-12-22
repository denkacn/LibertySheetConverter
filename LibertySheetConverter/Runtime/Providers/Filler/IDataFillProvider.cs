using System;
using System.Collections.Generic;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.Filler
{
    public interface IDataFillProvider
    {
        List<object> Fill(Type classType, FillDataContainer fillDataContainer);
    }
}
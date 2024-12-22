using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using LibertySheetConverter.Runtime.DataConverter;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.GoogleSheets
{
    public class GoogleSheetsProvider : IGoogleSheetsProvider
    {
        private const string CredPath = "token.json";

        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private readonly ConverterEngineContext _context;
        private readonly MainDataConverter _mainDataConverter;
        private readonly SheetsService _service;

        private Spreadsheet _tableData;

        public GoogleSheetsProvider(ConverterEngineContext context)
        {
            _context = context;
            _mainDataConverter = new MainDataConverter(_context.SettingData.CustomValueDataParsers);

            UserCredential credential;

            using (var stream = new FileStream(_context.SettingData.VarsData.PathToCredentials, FileMode.Open,
                       FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredPath, true)).Result;

                _context.Logger?.Log("Credential file saved to: " + CredPath);
            }

            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _context.SettingData.VarsData.ApplicationName,
            });
        }

        public async Task<bool> PreparingData()
        {
            await LoadTableData();

            var parseTableData = ParseTableData();
            var customConfigurationsData = TryGetCustomConfigurationData();
            
            _context.RuntimeData.ConfigurationsDataContainer = parseTableData;
            _context.RuntimeData.CustomConfigurationsData = customConfigurationsData;
            
            return true;
        }

        private async Task LoadTableData()
        {
            var request = _service.Spreadsheets.Get(_context.SettingData.VarsData.SheetId);
            request.IncludeGridData = true;
            _tableData = await request.ExecuteAsync();
        }

        private ConfigurationsDataContainer ParseTableData()
        {
            var configurationsDataContainer = new ConfigurationsDataContainer();

            foreach (var sheet in _tableData.Sheets)
            {
                if (string.IsNullOrEmpty(sheet.Properties.Title)) continue;

                var title = sheet.Properties.Title;
                
                if (_context.SettingData.VarsData.IgnoreTabs.Contains(title)) continue;

                if (_context.SettingData.VarsData.EnableTabs != null &&
                    _context.SettingData.VarsData.EnableTabs.Length > 0)
                {
                    if (!_context.SettingData.VarsData.EnableTabs.Contains(title)) continue;
                }
                
                configurationsDataContainer.ConfigurationNames.Add(title);
                
                _context.Logger?.Log($"Title: {title}");

                var fields = new List<FieldData>();
                var values = new List<FillDataValues>();

                foreach (var data in sheet.Data)
                {
                    if (!IsValidRowData(data.RowData)) continue;

                    var nameRowData = data.RowData[0];
                    var typeRowData = data.RowData[1];

                    ProcessHeaderRow(nameRowData, typeRowData, fields);
                    ProcessDataRows(data.RowData, typeRowData, values);

                    _context.Logger?.Log("------------------------");
                }

                var fillDataContainer = new FillDataContainer(fields, values);
                configurationsDataContainer.Add($"{title}Configuration", fillDataContainer);
            }

            return configurationsDataContainer;
        }

        private bool IsValidRowData(IList<RowData> rowData)
        {
            return rowData.Count >= 2;
        }

        private void ProcessHeaderRow(RowData nameRowData, RowData typeRowData, List<FieldData> fields)
        {
            for (var index = 0; index < nameRowData.Values.Count; index++)
            {
                var nameCellData = nameRowData.Values[index];
                var typeCellData = typeRowData.Values[index];

                if (nameCellData.UserEnteredValue == null || typeCellData.UserEnteredValue == null) continue;

                if (typeCellData.FormattedValue == "ignore") continue;

                var fieldName = nameCellData.FormattedValue.Replace(" ", "");
                var fieldType = typeCellData.FormattedValue;
                
                fields.Add(new FieldData(fieldName, fieldType));
            }
        }

        private void ProcessDataRows(IList<RowData> rowData, RowData typeRowData, List<FillDataValues> values)
        {
            for (var rowIndex = 2; rowIndex < rowData.Count; rowIndex++)
            {
                var currentRow = rowData[rowIndex];
                var fillDataValues = new FillDataValues();

                for (var cellIndex = 0; cellIndex < currentRow.Values.Count; cellIndex++)
                {
                    var cellData = currentRow.Values[cellIndex];
                    var typeCellData = typeRowData.Values[cellIndex];

                    var value = string.IsNullOrEmpty(cellData.FormattedValue) ? string.Empty : cellData.FormattedValue;

                    if (typeCellData.FormattedValue == "string")
                    {
                        value = MyRegex().Replace(value, "");
                    }

                    if (typeCellData.FormattedValue != "ignore")
                    {
                        fillDataValues.Add(_mainDataConverter.Convert(value, typeCellData.FormattedValue));
                    }

                    //_context.Logger?.Log($"cellData: {cellData.FormattedValue} | {typeCellData.FormattedValue}");
                }

                values.Add(fillDataValues);
            }
        }
        
        private List<CustomConfigurationData> TryGetCustomConfigurationData()
        {
            var customConfigurationsData = new List<CustomConfigurationData>();

            if (!string.IsNullOrEmpty(_context.SettingData.VarsData.CustomConfigurationsTabName))
            {
                foreach (var sheet in _tableData.Sheets)
                {
                    if (string.IsNullOrEmpty(sheet.Properties.Title)) continue;

                    var title = sheet.Properties.Title;
                    
                    if (title != _context.SettingData.VarsData.CustomConfigurationsTabName) continue;
                    
                    foreach (var data in sheet.Data)
                    {
                        if (!IsValidRowData(data.RowData)) continue;

                        var nameRowData = data.RowData[0];
                        var typeRowData = data.RowData[1];
                        
                        for (var index = 0; index < nameRowData.Values.Count; index++)
                        {
                            var nameCellData = nameRowData.Values[index];
                            var typeCellData = typeRowData.Values[index];

                            if (nameCellData.UserEnteredValue == null || typeCellData.UserEnteredValue == null) continue;

                            if (typeCellData.FormattedValue == "ignore") continue;

                            var configurationName = nameCellData.FormattedValue.Replace(" ", "");
                            var configurationData = typeCellData.FormattedValue;

                            customConfigurationsData.Add(new CustomConfigurationData($"{configurationName}Configuration", FixStringToUtf8(configurationData)));
                            
                        }
                    }
                }
            }
            
            return customConfigurationsData;
        }

        private string FixStringToUtf8(string originalString)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(originalString);
            return Encoding.UTF8.GetString(utf8Bytes);
        }
        
        private static Regex MyRegex() => new Regex(@"[^a-zA-Zа-яА-Я0-9\s]", RegexOptions.Compiled);
    }
}
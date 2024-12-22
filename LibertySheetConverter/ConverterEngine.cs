using System;
using System.Linq;
using System.Threading.Tasks;
using LibertySheetConverter.Runtime.Extensions;
using LibertySheetConverter.Runtime.Interfaces;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.DataContainer;
using LibertySheetConverter.Runtime.Providers.CodeGenerator;
using LibertySheetConverter.Runtime.Providers.Compiler;
using LibertySheetConverter.Runtime.Providers.ConfigurationGenerator;
using LibertySheetConverter.Runtime.Providers.Filler;
using LibertySheetConverter.Runtime.Providers.GoogleSheets;
using LibertySheetConverter.Runtime.Providers.Storage;

namespace LibertySheetConverter
{
    public class ConverterEngine : IConverterEngine
    {
        public ConverterEngineContext Context => _context;
        
        private readonly ConverterEngineContext _context;
        private readonly IGoogleSheetsProvider _googleSheetsProvider;
        private readonly ICodeGeneratorProvider _codeGeneratorProvider;
        private readonly ICompilerProvider _compilerProvider;
        private readonly IDataFillProvider _dataFillProvider;
        
        private IStorageProvider _storageProvider;
        private IConfigurationsGeneratorProvider _configurationsGeneratorProvider;
        
        private readonly ConverterEngineOptionsData _optionsData;
        
        public ConverterEngine(ConverterEngineSettingData settingData, ConverterEngineOptionsData optionsData, IConverterEngineLogger? logger = null)
        {
            _context = new ConverterEngineContext(settingData, logger);
            _optionsData = optionsData;
            
            _googleSheetsProvider = new GoogleSheetsProvider(_context);
            _storageProvider = new StorageProvider(_context);
            _codeGeneratorProvider = CreateCodeGeneratorProvider();
            _compilerProvider = CreateCompilerProvider();
            _dataFillProvider = new DataFillProvider();
            _configurationsGeneratorProvider = new JsonConfigurationsGeneratorProvider();
            
            _context.Logger?.Log("ConverterEngine created");
        }

        public void SetStorageProvider(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void SetConfigurationsGeneratorProvider(IConfigurationsGeneratorProvider configurationsGeneratorProvider)
        {
            _configurationsGeneratorProvider = configurationsGeneratorProvider;
        }

        private ICompilerProvider CreateCompilerProvider()
        {
            if(_optionsData.IsUseAutoDependencies) 
                return new CompilerProviderWithAutoDependencies(_context);
            else 
                return new CompilerProviderWithPathToDependencies(_context);
        }
        
        private ICodeGeneratorProvider CreateCodeGeneratorProvider()
        {
            if(_optionsData.IsUseCodeGeneratorWithAuroraData) 
                return new CodeGeneratorProviderWithAuroraDataContainer(_context);
            else 
                return new CodeGeneratorProviderSimple(_context);
        }
        
        public async Task<bool> Convert()
        {
            await PreparingData();
            await GenerateClasses();

            CompileClasses();
            FillData();

            await SaveConfigurations();
            
            return await Task.FromResult(true);
        }

        private async Task PreparingData()
        {
            try
            {
                await _googleSheetsProvider.PreparingData();
            }
            catch (Exception ex)
            {
                _context.Logger?.Log($"[ConverterEngine] Error PreparingDataFromTable: {ex.Message}");
            }
        }

        private async Task GenerateClasses()
        {
            try
            {
                foreach (var configuration in _context.RuntimeData.ConfigurationsDataContainer.Configurations)
                {
                    var classCode = _codeGeneratorProvider.Generate(configuration.ConfigurationName,
                        configuration.DataContainer.Fields);

                    if (!_context.SettingData.VarsData.IsCreateClasses)
                    {
                        await _storageProvider.Save(_context.SettingData.VarsData.ClassSavePath,
                            configuration.ConfigurationName,
                            _context.SettingData.VarsData.ClassExtension, classCode);
                    }
                    
                    _context.RuntimeData.GeneratedCode.Add(configuration.ConfigurationName, classCode);
                }
            }
            catch (Exception ex)
            {
                _context.Logger?.Log($"[ConverterEngine] Error GenerateClasses: {ex.Message}");
            }
        }
        
        private void CompileClasses()
        {
            try
            {
                _context.RuntimeData.CompileResultData = _compilerProvider.Compile(_context.RuntimeData.GeneratedCode);
            }
            catch (Exception ex)
            {
                _context.Logger?.Log($"[ConverterEngine] Error CompileClasses: {ex.Message}");
            }
        }
        
        private void FillData()
        {
            try
            {
                if (_context.RuntimeData.CompileResultData.IsSuccess)
                {
                    var assemblyTypes = _context.RuntimeData.CompileResultData.Assembly.GetTypes();
                    
                    _context.Logger?.Log(string.Join(";\n", assemblyTypes.Select(type => type.ToString())));
                    
                    foreach (var configuration in _context.RuntimeData.ConfigurationsDataContainer.Configurations)
                    {
                        var type = _context.RuntimeData.CompileResultData.Assembly.GetType(
                            _context.SettingData.VarsData.MainNameSpace + "." + configuration.ConfigurationName);
                        var filledConfigurationData = _dataFillProvider.Fill(type, configuration.DataContainer);

                        _context.RuntimeData.FilledConfigurationsData.Add(
                            new FilledConfigurationData(configuration.ConfigurationName, filledConfigurationData));
                    }
                }
                else
                {
                    _context.Logger?.Log("GenerateClasses Error ...");
                }
            }
            catch (Exception ex)
            {
                _context.Logger?.Log($"[ConverterEngine] Error FillData: {ex.Message}");
            }
        }

        private async Task SaveConfigurations()
        {
            try
            {
                if (!_context.SettingData.VarsData.IsCreateJson) return;
                
                foreach (var filledConfiguration in _context.RuntimeData.FilledConfigurationsData)
                {
                    var configurationString = _configurationsGeneratorProvider.GenerateConfigurationString(filledConfiguration);
                    
                    await _storageProvider.Save(_context.SettingData.VarsData.JsonSavePath,
                        filledConfiguration.ConfigurationName.ToSnakeCase(),
                        _context.SettingData.VarsData.JsonExtension, configurationString);
                }

                _context.Logger?.Log("GenerateConfigurationJson Done ...");
            }
            catch (Exception ex)
            {
                _context.Logger?.Log($"[ConverterEngine] Error FillData: {ex.Message}");
            }
        }
    }
}
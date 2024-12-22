using System;

namespace LibertySheetConverter.Runtime.Models
{
    public class ConverterEngineVarsData
    {
        //common
        public string ClassSavePath { get; set; } = "/GeneratedClasses";
        public string ClassExtension { get; set; } = ".cs";

        public string JsonSavePath { get; set; } = "GeneratedConfiguration";
        public string JsonExtension { get; set; } = ".json";
        
        public string LibsSavePath { get; set; } = "GeneratedLibs";
        public string LibsExtension { get; set; } = ".dll";
        
        public bool IsCreateClasses { get; set; } = true;
        public bool IsCreateJson { get; set; } = true;
        public bool IsCreateLibs { get; set; } = true;
        
        //google
        public string ApplicationName { get; set; }
        public string PathToCredentials { get; set; }
        public string SheetId { get; set; }
        
        public string[] IgnoreTabs { get; set; } = Array.Empty<string>();
        public string[] EnableTabs { get; set; } = Array.Empty<string>();
        
        //code generator
        public string DependenciesPath { get; set; } = string.Empty;
        public bool IsUseAutoDependencies { get; set; } = true;
        public string MainNameSpace { get; set; }
        public string[] CustomUsings { get; set; } = Array.Empty<string>();
        public string ConfigurationAssemblyName { get; set; } = "FastRtsConfiguration";
        public string[] CustomLibsPath { get; set; } = Array.Empty<string>();
        public string CustomConfigurationsTabName { get; set; } = "";
    }
}
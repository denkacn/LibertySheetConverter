using System.Collections.Generic;
using System.IO;
using LibertySheetConverter.Runtime.Models;
using LibertySheetConverter.Runtime.Models.DataContainer;

namespace LibertySheetConverter.Runtime.Providers.CodeGenerator
{
    public class CodeGeneratorProviderWithAuroraDataContainer : CodeGeneratorProviderBase
    {
        public CodeGeneratorProviderWithAuroraDataContainer(ConverterEngineContext context) : base(context){}
        
        public override string Generate(string className, List<FieldData> fields)
        {
            var code = new StringWriter();
            
            code.WriteLine("using System;");
            code.WriteLine("using System.Collections.Generic;");
            code.WriteLine("using AuroraDataContainer.Attributes;");
            code.WriteLine("using AuroraDataContainer.Interface;");

            foreach (var customUsing in _context.SettingData.VarsData.CustomUsings)
            {
                code.WriteLine($"using {customUsing};");
            }
            
            code.WriteLine("");
            code.WriteLine($"namespace {_context.SettingData.VarsData.MainNameSpace}"); 
            code.WriteLine("{");
            code.WriteLine($"    public class {className} {GetCustomNamespaceData(className)}");
            code.WriteLine("    {");

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                if (index == 0)
                {
                    code.WriteLine($"        [DataKey]  public {field.FieldType} {field.FieldName.Replace(" ", "")} {{ get; set; }}");
                }
                else
                {
                    code.WriteLine($"        public {field.FieldType} {field.FieldName.Replace(" ", "")} {{ get; set; }}");
                }
            }

            code.WriteLine("    }");
            code.WriteLine("}");
            
            return code.ToString();
        }
    }
}
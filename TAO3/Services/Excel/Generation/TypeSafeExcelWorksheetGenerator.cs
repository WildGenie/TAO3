﻿using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAO3.Internal.CodeGeneration;
using TAO3.Internal.Extensions;

namespace TAO3.Excel.Generation
{
    internal static class TypeSafeExcelWorksheetGenerator
    {
        public static string Generate(CSharpKernel cSharpKernel, ExcelWorksheet sheet)
        {
            string className = IdentifierUtils.ToPascalCase(sheet.Name);

            List<string> tables = sheet
                .Tables
                .Select(t => TypeSafeExcelTableGenerator.Generate(cSharpKernel, t))
                .Select((name, index) => $"public {name} {name} => new {name}(Tables[{index}]);")
                .ToList();

            string getTablesCode = string.Join(@"
        ", tables);

            string code = $@"using System;
using System.Collections.Generic;
using System.Linq;
using TAO3.Excel;

public class {className} : ExcelWorksheet
{{
    {getTablesCode}

    internal {className}(ExcelWorksheet worksheet)
        : base(worksheet)
    {{
    }}
}}";

            cSharpKernel.DeferCommand(new SubmitCode(code));

            return className;
        }
    }
}

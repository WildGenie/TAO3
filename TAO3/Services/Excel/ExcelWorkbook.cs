﻿using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TAO3.Excel.Generation;

namespace TAO3.Excel
{
    public class ExcelWorkbook
    {
        internal ExcelTypeSafeGenerator TypeGenerator { get; }
        internal Workbook Workbook { get; }
        public dynamic Instance => Workbook;

        public string Name => Workbook.Name;
        public string FullName => Workbook.FullName;
        public IReadOnlyList<ExcelWorksheet> Worksheets => Workbook
            .Sheets
            .Cast<Worksheet>()
            .Select(x => new ExcelWorksheet(TypeGenerator, x))
            .ToList();

        internal ExcelWorkbook(ExcelTypeSafeGenerator typeGenerator, Workbook workbook)
        {
            TypeGenerator = typeGenerator;
            Workbook = workbook;
        }

        protected ExcelWorkbook(ExcelWorkbook workbook)
        {
            TypeGenerator = workbook.TypeGenerator;
            Workbook = workbook.Workbook;
        }

        public ExcelWorksheet AddWorksheet(string? name = null, bool refreshTypes = true)
        {
            Worksheet worksheet = Workbook.Sheets.Add(After: Workbook.Sheets[Workbook.Sheets.Count]);
            if (name != null)
            {
                worksheet.Name = name;
            }

            if (refreshTypes)
            {
                TypeGenerator.RefreshGeneration();
            }

            return new ExcelWorksheet(TypeGenerator, worksheet);
        }

        public void Save()
        {
            Workbook.Save();
        }

        public void SaveAs(string path)
        {
            Workbook.SaveAs2(path);
        }

        public void Close(bool refreshTypes = true)
        {
            Workbook.Close();

            if (refreshTypes)
            {
                TypeGenerator.RefreshGeneration();
            }
        }
    }
}

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExcelRosterReader
{
    abstract class TypeMap
    {
        public Type DataType { get; protected set; }
        public string SheetName { get; protected set; }
        public bool Required { get; set; }
        public abstract IEnumerable FromSheets(IEnumerable<Sheet> sheets);

        private IEnumerable<PropertyInfo> GetProperties()
        {
            return DataType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p=>p.CanRead && p.CanWrite && Type.GetTypeCode(DataType) >= TypeCode.Boolean);
        }

        public IXLWorksheet Create(XLWorkbook workbook)
        {
            var returnVar = workbook.AddWorksheet(DataType.Name);
            var firstRow = returnVar.Row(1);
            int i = 1;
            foreach (var p in GetProperties())
            {
                var cell = firstRow.Cell(i++);
                cell.Value = p.Name;
                cell.DataType = XLCellValues.Text;
            }
            return returnVar;
        }
        public Sheet Create(Sheet blankTemplate)
        {
            var returnVar = (Sheet)blankTemplate.CloneNode(deep: true);
            returnVar.Name = SheetName;
            var sheetData = returnVar.GetFirstChild<SheetData>();
            var row = sheetData.Elements<Row>().FirstOrDefault()
                ?? sheetData.AppendChild(new Row());

            foreach(var p in GetProperties())
            { 
                var cell = row.Elements<Cell>().FirstOrDefault()
                    ?? row.AppendChild(new Cell());
                cell.DataType.Value = CellValues.String;
                cell.CellValue.Text = p.Name;
            }
            return returnVar;
        }
    }
    class TypeMap<T> : TypeMap where T : class, new()
    {
        public TypeMap() : this(typeof(T).Name + 's')
        { }
        public TypeMap(string sheetName)
        {
            DataType = typeof(T);
            //?todo add pleuraliser
            SheetName = sheetName;
        }
        
        public override IEnumerable FromSheets(IEnumerable<Sheet> sheets)
        {
            var sheet = sheets.FirstOrDefault(s => string.Equals(s.Name?.Value?.Trim(), SheetName, StringComparison.InvariantCultureIgnoreCase));
            if (sheet == null)
            {
                return null;
            }
            return FromSheet.TypeFromSheet<T>(sheet);
        }
    }
}

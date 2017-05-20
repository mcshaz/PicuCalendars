using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ExcelParser
{
    public static class FromSheet
    {
        public static Dictionary<string, string> DictionaryFromSheet(Sheet sheet, string keyHeader, string valueHeader)
        {
            var rows = sheet.GetFirstChild<SheetData>().Elements<Row>();

            string keyCol = null;
            string valueCol = null;

            foreach (var c in rows.First().Elements<Cell>())
            {
                if (c.CellValue.Text == keyHeader)
                {
                    keyCol = GetColLetter(c);
                    if (valueCol != null) { break; }
                }
                else if(c.CellValue.Text == valueHeader)
                {
                    valueCol = GetColLetter(c);
                    if (keyCol != null) { break; }
                }
            }

            var returnVar = new Dictionary<string, string>();

            foreach(var r in rows.Skip(1))
            {
                string k = null, v = null;
                foreach (var c in r.Elements<Cell>())
                {
                    string colLetter = GetColLetter(c);
                    if (colLetter == valueCol)
                    {
                        v = c.CellValue.Text;
                        if (string.IsNullOrEmpty(v) || k != null) { break; }
                    }
                    else if (colLetter == keyCol)
                    {
                        k = c.CellValue.Text;
                        if (string.IsNullOrEmpty(k) || v != null) { break; }
                    }
                }
                if (k!=null && v != null)
                {
                    returnVar.Add(k, v);
                }
            }
            return returnVar;
        }

        public static List<T> TypeFromSheet<T>(Sheet sheet) where T : class, new()
        {
            /*
            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            {
                // Retrieve a reference to the workbook part.
                WorkbookPart wbPart = document.WorkbookPart;

                var rows = wbPart.Workbook.Descendants<Sheet>()
                */
            var rows = sheet.GetFirstChild<SheetData>().Elements<Row>();

            var colToPropInfo = new Dictionary<string, PropertyInfo>();
            foreach (var c in rows.First().Elements<Cell>())
            {
                if (!string.IsNullOrEmpty(c.CellValue.Text))
                {
                    var pi = typeof(T).GetProperty(c.CellValue.Text);
                    if (pi != null)
                    {
                        colToPropInfo.Add(GetColLetter(c), pi);
                    }
                }
            }

            var returnVar = new List<T>();

            foreach (var r in rows.Skip(1))
            {
                T rowInst = null;
                foreach (var c in r.Elements<Cell>())
                {
                    if (!string.IsNullOrEmpty(c.CellValue?.Text) && colToPropInfo.TryGetValue(GetColLetter(c), out PropertyInfo info))
                    {
                        object val;
                        if (c.DataType?.Value == CellValues.Date)
                        {
                            val = DateTime.FromOADate(double.Parse(c.CellValue.Text));
                        }
                        else if (info.PropertyType == typeof(string))
                        {
                            val = c.CellValue.Text;
                        }
                        else
                        {
                            var converter = TypeDescriptor.GetConverter(info.PropertyType);
                            val = converter.ConvertFromString(c.CellValue.Text);
                        }
                        info.SetValue(rowInst ?? (rowInst = new T()), val);
                    }
                }
                if (rowInst != null)
                {
                    returnVar.Add(rowInst);
                }
            }
            return returnVar;
        }

        public static string GetColLetter(Cell cell)
        {
            string address = cell.CellReference.Value;
            int i = 1;
            while (!char.IsDigit(address[i])) { i++; }
            return address.Substring(0, i);
        }
    }
}

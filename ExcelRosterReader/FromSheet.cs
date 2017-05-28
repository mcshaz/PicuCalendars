using ClosedXML.Excel;
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
        public static Dictionary<string, string> DictionaryFromSheet(IXLWorksheet ws, string keyHeader, string valueHeader)
        {
            var usedRows = ws.RangeUsed().RowsUsed();

            // Narrow down the row so that it only includes the used part
            int keyCol = 0;
            int valueCol = 0;
            foreach (var c in usedRows.First().CellsUsed())
            {
                if (c.GetString() == keyHeader)
                {
                    keyCol = c.Address.ColumnNumber;
                    if (valueCol != 0) { break; }
                }
                else if (c.GetString() == valueHeader)
                {
                    valueCol = c.Address.ColumnNumber;
                    if (keyCol != 0) { break; }
                }
            }
            // Move to the next row (it now has the titles)

            var returnVar = new Dictionary<string,string>();

            foreach (var r in usedRows.Skip(1))
            {
                string key = r.Cell(keyCol)?.GetString();
                string value = r.Cell(valueCol)?.GetString();
                if (!(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)))
                {
                    returnVar.Add(key, value);
                }
            }

            return returnVar;
        }

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

        public static List<T> TypeFromSheet<T>(IXLWorksheet ws, Guid? rosterId = null) where T : class, new()
        {
            var usedRows = ws.RangeUsed().RowsUsed();

            // Narrow down the row so that it only includes the used part
            var colToPropInfo = (from c in usedRows.First().CellsUsed()
                                 let pi = typeof(T).GetProperty(c.GetString())
                                 where pi != null
                                 select new { c.Address.ColumnNumber, pi }).ToDictionary(a=>a.ColumnNumber, a=>a.pi);

            // Move to the next row (it now has the titles)

            var returnVar = new List<T>();

            foreach (var r in usedRows.Skip(1))
            {
                T rowInst = null;
                foreach (var c in r.CellsUsed())
                {
                    if (colToPropInfo.TryGetValue(c.Address.ColumnNumber, out PropertyInfo info))
                    {
                        object val;
                        if (info.PropertyType == typeof(TimeSpan))
                        { 
                            if (c.DataType == XLCellValues.DateTime)
                            {
                                val = c.GetDateTime() - DateTime.FromOADate(0.0);
                            }
                            else
                            {
                                val = c.GetTimeSpan();
                            }
                            
                        }
                        else if (info.PropertyType == typeof(DateTime))
                        {
                            val = c.GetDateTime();
                        }
                        else if (info.PropertyType == typeof(string))
                        {
                            val = c.GetString();
                        }
                        else
                        {
                            var converter = TypeDescriptor.GetConverter(info.PropertyType);
                            val = converter.ConvertFromString(c.GetString());
                        }
                        info.SetValue(rowInst ?? (rowInst = new T()), val);
                    }
                }
                if (rowInst != null)
                {
                    if (rosterId.HasValue)
                    {
                        typeof(T).GetProperty("RosterId").SetValue(rowInst, rosterId.Value);
                    }
                    returnVar.Add(rowInst);
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

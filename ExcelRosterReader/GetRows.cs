using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using PicuCalendars.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelParser
{
    class GetRows
    {
        public static IEnumerable<Appointment> FromInitialDict(XLWorkbook workbook, Dictionary<string, string> columnShiftLink, string dateCol = "A")
        {
            // Open the spreadsheet document for read-only access.
            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            int year = DateTime.Today.Year;

            string[] years = new[] { year.ToString(), (year + 1).ToString() };

            var sheets = workbook.Worksheets
                .Where(s => years.Contains(s.Name));

            List<Appointment> shiftModels = new List<Appointment>();

            char[] splitters = new[] { ',', '/' };

            foreach (var s in sheets)
            {
                var rows = s.RowsUsed();
                foreach (var row in rows)
                {
                    if (row.Cell(dateCol).TryGetValue(out DateTime rowDate))
                    {
                        var rowShifts = new List<Tuple<string, string>>();
                        foreach (var kv in columnShiftLink)
                        {
                            if (row.Cell(kv.Key).TryGetValue(out string staffCode) && !string.IsNullOrEmpty(staffCode))
                            {
                                rowShifts.AddRange(staffCode.Split(splitters).Select(sc => new Tuple<string, string>(kv.Value, sc)));
                            }
                        }
                        shiftModels.AddRange(rowShifts.GroupBy(rs => rs.Item1)
                            .Select(g => new Appointment { Date = rowDate, ShiftCode = g.Key, StaffInitials = g.Select(rs => rs.Item2).ToArray() }));
                    }
                }
            }
            return shiftModels;
        }
        public static IEnumerable<Appointment> FromInitialDict(Workbook workbook, Dictionary<string, string> columnShiftLink, string dateCol = "A")
        {
            // Open the spreadsheet document for read-only access.
            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            int year = DateTime.Today.Year;

            string[] years = new[] { year.ToString(), (year + 1).ToString() };

            var sheets = workbook.Descendants<Sheet>()
                .Where(s => years.Contains(s.Name?.Value));

            List<Appointment> shiftModels = new List<Appointment>();

            char[] splitters = new[] { ',', '/' };

            foreach (var s in sheets)
            {
                var rows = s.GetFirstChild<SheetData>().Elements<Row>();
                foreach (var row in rows)
                {
                    DateTime? rowDate = null;
                    List<Appointment> rowShifts = new List<Appointment>();
                    var cells = row.Elements<Cell>();
                    foreach (var cell in cells)
                    {
                        string colLetter = FromSheet.GetColLetter(cell);
                        if (colLetter == dateCol)
                        {
                            if (cell.DataType.Value == CellValues.Date)
                            {
                                rowDate = DateTime.FromOADate(double.Parse(cell.CellValue.Text));
                            }
                            else
                            {
                                break;
                            }

                        }
                        else if (!string.IsNullOrEmpty(cell.CellValue.Text) && columnShiftLink.TryGetValue(colLetter, out string shift))
                        {
                            rowShifts.Add(new Appointment { ShiftCode = shift, StaffInitials = cell.CellValue.Text.Split(splitters) });
                        }
                    }
                    if (rowDate.HasValue)
                    {
                        foreach (var rs in rowShifts)
                        {
                            rs.Date = rowDate.Value;
                        }
                        shiftModels.AddRange(rowShifts);
                    }
                }
            }
            return shiftModels;
        }
    }
}

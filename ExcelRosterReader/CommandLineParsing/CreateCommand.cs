using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.CommandLineUtils;
using PicuCalendars.DataAccess;
using PicuCalendars.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader.CommandLineParsing
{
    public class CreateCommand : CommandLineApplication
    {
        private readonly CommandOption _rosterFilePathCmd;
        private readonly CommandOption _mapFilePathCmd;
        private readonly CommandOption _rosterTypeHeaderCmd;
        private readonly CommandOption _rosterTypeColumnsCmd;
        private readonly CommandOption _departmentCmd;
        private readonly CommandOption _dateColumnCmd;
        private readonly CommandArgument _arg;

        public CreateCommand()
        {
            Name = "create";
            FullName = "create new roster";
            Description = "create a new roster on the server, and set default values for syncs and upserts";
            Syntax = $"create <description>";

            _arg = Argument("description", "description of newly created roster file info");
            _rosterFilePathCmd = Option("-r | --roster", "the file path containing the excel roster", CommandOptionType.SingleValue);
            _mapFilePathCmd = Option("-m | --map", "the file path containing the excel sheets with column maps, staff details & shift times. If ommitted, --roster option is used for both", CommandOptionType.SingleValue);
            _rosterTypeHeaderCmd = Option("-s | --shifts", "The type of roster file - staff codes are horizontal headers, cell values are shift codes", CommandOptionType.NoValue);
            _rosterTypeColumnsCmd = Option("-c | --columns", "The type of roster file - staff codes are cell values [default], a worksheet 'map' links columns to shift codes", CommandOptionType.NoValue);
            _departmentCmd = Option("-u | --unit", "The name of the unit/deparment", CommandOptionType.SingleValue);
            _dateColumnCmd = Option("-d | --date", "The column containing the date - default 'A'", CommandOptionType.SingleValue);

            HelpOption("-? | -h | --help");
            OnExecute((Func<int>)RunCommand);
        }

        public CreateCommand(CommandOption rosterTypeHeaderCmd)
        {
            _rosterTypeHeaderCmd = rosterTypeHeaderCmd;
        }

        private int RunCommand()
        {
            string rosterPath = _rosterFilePathCmd.Value();
            if (string.IsNullOrEmpty(rosterPath))
            {
                Error.WriteLine($"The roster option ({_rosterFilePathCmd.Template}) must be specified");
                return 1;
            }

            if (!ExcelFileOk(rosterPath, "roster"))
            {
                return 1;
            }

            string departmentName = _departmentCmd.Value();
            if (string.IsNullOrEmpty(departmentName))
            {
                Error.WriteLine($"A department name ({_departmentCmd.Template}) must be specified");
                return 1;
            }

            string dateCol = _dateColumnCmd.Value();
            if (string.IsNullOrEmpty(dateCol))
            {
                Out.WriteLine("No datecol option specified - defaulting to A");
                dateCol = "A";
            }

            string mapPath = _mapFilePathCmd.Value();
            XLWorkbook wb;
            bool isNew = false;
            if (string.IsNullOrEmpty(mapPath))
            {
                Out.WriteLine("No map specified - using roster file");
                wb = new XLWorkbook(rosterPath);
            }
            else if (File.Exists(mapPath))
            {
                if (!ExcelFileOk(mapPath,"map"))
                {
                    return 1;
                }
                wb = new XLWorkbook(mapPath);
            }
            else
            {
                if (!ExtensionOk(mapPath,"map"))
                {
                    return 1;
                }
                wb = new XLWorkbook();
                isNew = true;
            }
            AddSheets(wb);
            if (isNew)
            {
                wb.SaveAs(mapPath);
            }
            else
            {
                wb.Save();
            }

            bool headerType = _rosterTypeHeaderCmd.Value() != null;
            if (headerType && _rosterTypeColumnsCmd.Value() != null)
            {
                Error.WriteLine("Must specify either headers or columns option, but not both");
            }
            var secret = CryptoUtilities.GenerateKey();
            var newInfo = new ExcelRosterFileInfo {
                Description = _arg.Value,
                Base64Secret = Convert.ToBase64String(secret),
                MapPath = mapPath,
                RosterPath = rosterPath,
                RosterId = Guid.NewGuid(),
                RosterType = headerType ? ExcelRosterFileInfo.RosterTypes.HorizontallyListedNames : ExcelRosterFileInfo.RosterTypes.ImplicitNames,
                DateColumn = dateCol
            };

            var res = SendEntities.CreateRoster(new Roster
            {
                Id = newInfo.RosterId,
                RosterName = Description,
                Secret = secret,
                DepartmentName = departmentName
            });

            if (!res.IsSuccessStatusCode)
            {
                Error.WriteLine(res.ReasonPhrase);
                //Error.WriteLine(res.Content)
                return 1;
            }
            Storage.Add(newInfo);
            return 0;
        }

        private bool ExtensionOk(string path, string description)
        {
            var ext = Path.GetExtension(path);
            if (!(ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".xlsm", StringComparison.OrdinalIgnoreCase)))
            {
                Error.WriteLine($"{description} is not a valid extension (must be .xlsx or .xlsm})");
                return false;
            }
            return true;
        }
        private bool ExcelFileOk(string path, string description)
        {
            if (!ExtensionOk(path, description))
            {
                return false;
            }
            SpreadsheetDocument document;
            try
            {
                document = SpreadsheetDocument.Open(path, false);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                var type = e.GetType();
                string msg = (type == typeof(FileFormatException))
                    ? "file is not in a valid xls? format"
                    : e.Message;
                Error.WriteLine($"{description} returned {type.Name}: {msg}");
                //document will be null;
                return false;
            }
            document.Dispose();
            return true;
        }

        void AddSheets(XLWorkbook document)
        { 
            var requiredSheets = App._typeMaps.ToDictionary(t=>t.SheetName,StringComparer.InvariantCultureIgnoreCase);
            if (_rosterTypeHeaderCmd.Value() == null)
            {
                var colMap = new TypeMap<ColumnMap>();
                requiredSheets.Add(colMap.SheetName,colMap);
            }

            var sheets = document.Worksheets;
            
            foreach (var s in sheets)
            {
                requiredSheets.Remove(s.Name);
            }

            foreach (var s in requiredSheets.Values)
            {
                s.Create(document);
            }
        }
    }
}

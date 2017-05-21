using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelParser;
using ExcelRosterReader.CommandLineParsing;
using Microsoft.Extensions.CommandLineUtils;
using PicuCalendars.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader
{
    class UpsertCommand : CommandLineApplication
    {
        private readonly CommandArgument _arg;
        private readonly CommandOption _tableOption;
        const string _allOption = "All";

        public UpsertCommand()
        {
            Name = "upsert";
            FullName = "upsert table";
            Description = "upsert all rows from given excel worksheet onto web server";
            var rosters = Storage.GetRosters().Select(s=>s.Description);
            var entityTypeNames = string.Join("|", App._typeMaps.Select(e => e.SheetName)) + '|' + _allOption + "[default]";
            Syntax = $"upsert <{string.Join("|",rosters)}>";

            _arg = Argument("roster", "name of the roster to upsert",multipleValues:false);
            _tableOption = Option("-t | --table", "The table to upsert " + entityTypeNames, CommandOptionType.SingleValue);

            HelpOption("-? | -h | --help");
            OnExecute((Func<int>)RunCommand);
        }

        protected int RunCommand()
        {
            var rosterInfo = Storage.Find(_arg.Value);
            if (rosterInfo == null)
            {
                Error.WriteLine("unknown roster - use syntax " + Syntax);
                return 1;
            }

            string tableName = _tableOption.Value();
            if (string.IsNullOrEmpty(tableName))
            {
                Out.WriteLine("No table name specified - default to " + _allOption + " option");
                tableName = _allOption;
            }
            IEnumerable<TypeMap> selectedTypes = tableName.Equals(_allOption, StringComparison.InvariantCultureIgnoreCase)
                ? selectedTypes = App._typeMaps
                :App._typeMaps.Where(e => e.SheetName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (!selectedTypes.Any())
            {
                Error.Write("An entity with that name was not found - use syntax " + Syntax);
                return 1;
            }

            using (var document = SpreadsheetDocument.Open(rosterInfo.MapPath, false))
            {
                var sheets = document.WorkbookPart.Workbook.Descendants<Sheet>();
                foreach (var s in selectedTypes)
                {
                    var data = s.FromSheets(sheets);
                    SendEntities.PostRosterUpsert(rosterInfo.RosterId, rosterInfo.Base64Secret, data, Out, Error);
                }
            }

            return 0;
        }
    }
}

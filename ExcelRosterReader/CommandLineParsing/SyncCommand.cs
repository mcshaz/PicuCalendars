using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelParser;
using Microsoft.Extensions.CommandLineUtils;
using PicuCalendars.DataAccess;
using PicuCalendars.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader
{
    class SyncCommand : CommandLineApplication
    {
        private readonly CommandArgument _arg;
        const string _allOption = "All";

        public SyncCommand()
        {
            Name = "sync";
            FullName = "Syncronise rosters";
            Description = "upload excel roster to web server";
            var rosters = Storage.GetRosters().Select(s => s.Description) + $"|{_allOption}[default]";
            Syntax = $"sync <{string.Join("|", rosters)}>";
            _arg = Argument("roster", "name of the roster to upsert", multipleValues: true);

            HelpOption("-? | -h | --help");
            OnExecute((Func<int>)RunCommand);
        }

        protected int RunCommand()
        {
            IEnumerable<ExcelRosterFileInfo> rosters;
            if (_arg.Values.Count == 0)
            {
                Out.WriteLine("No table name specified - default to All");
                rosters = Storage.GetRosters();
            }
            else if (_allOption.Equals(_arg.Values[0], StringComparison.InvariantCultureIgnoreCase))
            {
                rosters = Storage.GetRosters();
            }
            else
            {
                var r = new List<ExcelRosterFileInfo>(_arg.Values.Count);
                foreach(var v in _arg.Values)
                {
                    var rfi = Storage.Find(v);
                    if (rfi == null)
                    {
                        Out.WriteLine("Unknown roster " + v);
                        return 1;
                    }
                    r.Add(rfi);
                }
                rosters = r;
            }
            foreach (var r in rosters)
            {
                if (r.RosterType == ExcelRosterFileInfo.RosterTypes.ImplicitNames)
                {
                    var sheetName = new TypeMap<ColumnMap>().SheetName;
                    using (var mapSS = new XLWorkbook(r.MapPath, XLEventTracking.Disabled))
                    {
                        var sheet = mapSS.Worksheets.FirstOrDefault(s=> s.Name.Equals(sheetName,StringComparison.InvariantCultureIgnoreCase));
                        var map = FromSheet.DictionaryFromSheet(sheet, "Column", "ShiftCode");
                        IEnumerable<Appointment> roster;
                        if (r.MapPath == r.RosterPath)
                        {
                            roster = GetRows.FromInitialDict(mapSS, map, r.DateColumn);
                        }
                        else
                        {
                            using (var rosterSS = new XLWorkbook(r.RosterPath, XLEventTracking.Disabled))
                            {
                                roster = GetRows.FromInitialDict(rosterSS, map, r.DateColumn);
                            }
                        }
                        SendEntities.PostRosterUpsert(r.RosterId, r.Base64Secret, roster, Out, Error);
                    }
                }
            }
            return 0;
        }
    }
}

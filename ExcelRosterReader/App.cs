using Microsoft.Extensions.CommandLineUtils;
using PicuCalendars.DataAccess;
using System.Collections;

namespace ExcelRosterReader.CommandLineParsing
{
    class App : CommandLineApplication
    {
        public App()
        {            
            Commands.Add(new CreateCommand());
            Commands.Add(new SyncCommand());
            Commands.Add(new UpsertCommand());
            HelpOption("-h | -? | --help");
        }

        internal static TypeMap[] _typeMaps = (new TypeMap[]
        {
            new TypeMap<Shift>{ Required = true },
            new TypeMap<StaffMember>(),
            // new TypeMap<ColumnMap>()
        });
    }
}

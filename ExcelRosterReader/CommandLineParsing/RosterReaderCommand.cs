using System;
using static System.Console;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;
using System.IO;

namespace ExcelRosterReader
{
    public abstract class RosterReaderCommand : CommandLineApplication
    {
        private readonly CommandOption _domain;

        private string _rosterFilePath;
        private string _mapFilePath;

        public RosterReaderCommand()
        {
            _domain = Option("-d | --domain", "The domain to post values to", CommandOptionType.SingleValue);

            HelpOption("-? | -h | --help");
            OnExecute((Func<int>)RunCommand);
        }

        protected Uri Domain {
            get
            {
                var domain = _domain.Value();
                if (string.IsNullOrEmpty(domain))
                {
#if DEBUG
                    domain = "http://localhost:51558";
#else
                    domain = "http://rosters.sim-planner.com";
#endif
                }
                return new Uri(domain);
            }
        }

        protected abstract int RunCommand();
    }
}

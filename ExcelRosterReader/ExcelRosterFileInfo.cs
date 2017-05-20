using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelRosterReader
{
    class ExcelRosterFileInfo
    {
        public enum RosterTypes { HorizontallyListedNames, ImplicitNames }

        public string RosterPath { get; set; }
        public string MapPath { get; set; }
        [BsonIndex]
        public string Description { get; set; }
        [BsonId]
        public Guid RosterId { get; set; }
        public string Base64Secret { get; set; }
        public RosterTypes RosterType { get; set; }
        public string DateColumn { get; set; }
    }
}

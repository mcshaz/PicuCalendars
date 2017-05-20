using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace ExcelRosterReader
{
    class Storage
    {
        const string dbFile = "rosterDetails.db";
        public static IEnumerable<ExcelRosterFileInfo> GetRosters()
        {
            using (var db = new LiteDatabase(dbFile))
            {
                return db.GetCollection<ExcelRosterFileInfo>()
                    .FindAll();
                // now we can carry out CRUD operations on the data
            }
        }

        public static void Add(ExcelRosterFileInfo rosterInfo)
        {
            using (var db = new LiteDatabase(dbFile))
            {
                db.GetCollection<ExcelRosterFileInfo>()
                    .Insert(rosterInfo);
            }
        }

        public static ExcelRosterFileInfo Find(string description)
        {
            using (var db = new LiteDatabase(dbFile))
            {
                return db.GetCollection<ExcelRosterFileInfo>()
                    .Find(er=>er.Description == description, 0,1).Single();
                // now we can carry out CRUD operations on the data
            }
        }
    }
}

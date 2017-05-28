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

        public static void KeepLast()
        {
            using (var db = new LiteDatabase(dbFile))
            {
                var col = db.GetCollection<ExcelRosterFileInfo>();
                var last = col.FindAll().Last();
                col.Delete(fi => fi.RosterId != last.RosterId);
            }
        }

        public static ExcelRosterFileInfo Find(string description)
        {
            using (var db = new LiteDatabase(dbFile))
            {
                return db.GetCollection<ExcelRosterFileInfo>()
                    .Find(er=>er.Description == description).SingleOrDefault();
                // now we can carry out CRUD operations on the data
            }
        }
    }
}

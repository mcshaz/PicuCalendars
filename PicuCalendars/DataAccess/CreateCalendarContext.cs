using EFExtensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.DataAccess
{
    public class CreateCalendarContext : DropCreateDatabaseIfModelChanges<CalendarContext>
    {
        protected override void Seed(CalendarContext context)
        {
            /*
            var cons = new ServerRoster
            {
                Id = Guid.Parse("3C6C9917-2C3D-4E9A-8CE1-60CA9CE3695D"),
                DepartmentName = "SS PICU",
                RosterName = "Consultants",
                Secret = Convert.FromBase64String("hJ4blpw8qTI5PEFpdzM2cNpnf6qTNzSBys+x8zQjjojcrVLvPPrXbmpg1ICXgYAg7AprMxz5EiOId9qOR8avzw==")
            };
            context.Rosters.Add(cons);

            var ts8hr = TimeSpan.FromHours(8.0);
            var ts16hr = TimeSpan.FromHours(16.0);
            context.Shifts.AddRange(new[] {
                new Shift { Roster = cons, Code="D1", ShiftStart= ts8hr, Duration = ts8hr, Description="1st Call" },
                new Shift { Roster = cons, Code="D2", ShiftStart= ts8hr, Duration = ts8hr, Description="2nd Call" },
                new Shift { Roster = cons, Code="E1", ShiftStart= ts16hr, Duration = ts16hr, Description="1st Call" },
                new Shift { Roster = cons, Code="E2", ShiftStart= ts16hr, Duration = ts16hr, Description="2nd Call" },
                new Shift { Roster = cons, Code="LE", ShiftStart= new TimeSpan(0), Duration = TimeSpan.FromDays(1.0), Description = "Roster marked as leave", LeaveShift = true }
            });

            context.Staff.AddRange(new[] {
                new ServerStaffMember { /*Id = Guid.Parse("632036E2-C3CF-4116-B9B9-59AB075AB0DA"), FullName = "Alex Hussey",  Roster = cons, RosterCode="AH" },
                new ServerStaffMember { /*Id = Guid.Parse("1B435B77-0062-411F-9B6B-1B3C154D2E6D"), FullName = "Anusha Ganeshalingham",  Roster = cons, RosterCode="AG" },
                new ServerStaffMember { /*Id = Guid.Parse("BFE2AE9C-2C9A-4730-8D20-4BBC370461AB"), FullName = "Brent McSharry",  Roster = cons, RosterCode="BM" },
                new ServerStaffMember { /*Id = Guid.Parse("A1F39FFD-1161-4474-8823-8BB441395A2A"), FullName = "Fiona Miles",  Roster = cons, RosterCode="M" },
                new ServerStaffMember { /*Id = Guid.Parse("F4DE1C5A-AA18-4A99-B300-6183000CDC93"), FullName = "Gabrielle Nuthall",  Roster = cons, RosterCode="N" },
                new ServerStaffMember { /*Id = Guid.NewGuid(), FullName = "Dave Buckley",  Roster = cons, RosterCode="BU" },
                new ServerStaffMember { /*Id = Guid.NewGuid(), FullName = "John Beca",  Roster = cons, RosterCode="BE" },
                new ServerStaffMember { /*Id = Guid.NewGuid(), FullName = "Brian Anderson",  Roster = cons, RosterCode="A" },
                new ServerStaffMember { /*Id = Guid.NewGuid(), FullName = "Liz Segedin",  Roster = cons, RosterCode="S" }
            });

            context.SaveChanges();
    */
        }
    }
}
/* 
 * SELECT TOP 1000 'new StaffMember { Id = Guid.Parse("' + convert(nvarchar(36),[Id]) + '"), FullName = "' + [FullName] + '",  Department = picu, Abbreviation="" },'
  FROM [MedSimData].[dbo].[AspNetUsers]
  where DefaultDepartmentId = {guid'3C6C9917-2C3D-4E9A-8CE1-60CA9CE3695D'} and DefaultProfessionalRoleId in ({guid'3295C19A-B143-404B-9979-7572D0B77155'}, {guid'1162E5A3-9A2D-4A6D-8102-64D28F760DFB'})

new StaffMember { Id = Guid.Parse("632036E2-C3CF-4116-B9B9-59AB075AB0DA"), FullName = "Alex Hussey",  Department = picu, Abbreviation="AH" },
new StaffMember { Id = Guid.Parse("1B435B77-0062-411F-9B6B-1B3C154D2E6D"), FullName = "Anusha Ganeshalingham",  Department = picu, Abbreviation="AG" },
new StaffMember { Id = Guid.Parse("BFE2AE9C-2C9A-4730-8D20-4BBC370461AB"), FullName = "Brent McSharry",  Department = picu, Abbreviation="BM" },
new StaffMember { Id = Guid.Parse("A1F39FFD-1161-4474-8823-8BB441395A2A"), FullName = "Fiona Miles",  Department = picu, Abbreviation="M" },
new StaffMember { Id = Guid.Parse("F4DE1C5A-AA18-4A99-B300-6183000CDC93"), FullName = "Gabrielle Nuthall",  Department = picu, Abbreviation="N" },
*/

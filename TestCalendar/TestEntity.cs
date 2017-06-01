using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicuCalendars.DataAccess;
using System.Linq;
using EFExtensions;

namespace TestCalendar
{
    [TestClass]
    public class TestEntity
    {
        [TestMethod]
        public void TestCanCreate()
        {
            using (var c = new CalendarContext("Data Source=localhost;Initial Catalog=StaffCalendars;Integrated Security=True"))
            {
                var a = c.Appointments.Any();
            }
        }

        [TestMethod]
        public void TestUpsert()
        {
            using (var context = new CalendarContext("Data Source=localhost;Initial Catalog=StaffCalendars;Integrated Security=True"))
            {
                var cons = context.Rosters.FirstOrDefault();
                if (cons == null)
                {
                    cons = new ServerRoster
                    {
                        Id = Guid.Parse("D43880FF-6368-442D-85D6-01BB5CC6523C"),
                        DepartmentName = "SS PICU",
                        RosterName = "Consultants",
                        Secret = HexadecimalStringToByteArray_Original("2CBC90D518D6066F579EC5E85F77C366D63FEF1616A4DD9D6646ED42BFC16C694B53221DB55627305213B3280F591EEB171610EE7013C20F76AB5057EA36A59E") //Convert.FromBase64String("hJ4blpw8qTI5PEFpdzM2cNpnf6qTNzSBys+x8zQjjojcrVLvPPrXbmpg1ICXgYAg7AprMxz5EiOId9qOR8avzw==") //  0x2CBC90D518D6066F579EC5E85F77C366D63FEF1616A4DD9D6646ED42BFC16C694B53221DB55627305213B3280F591EEB171610EE7013C20F76AB5057EA36A59E 
                    };
                    context.Rosters.Add(cons);

                    context.SaveChanges();
                }

                context.Upsert(new[] {
                    new ServerStaffMember { /*Id = Guid.Parse("632036E2-C3CF-4116-B9B9-59AB075AB0DA"), */ FullName = "Alex Hussey",  Roster = cons, StaffMemberCode="AH" },
                    new ServerStaffMember { /*Id = Guid.Parse("1B435B77-0062-411F-9B6B-1B3C154D2E6D"), */ FullName = "Anusha Ganeshalingham",  Roster = cons, StaffMemberCode="AG" },
                    new ServerStaffMember { /*Id = Guid.Parse("BFE2AE9C-2C9A-4730-8D20-4BBC370461AB"), */ FullName = "Brent McSharry",  Roster = cons, StaffMemberCode="BM" },
                    new ServerStaffMember { /*Id = Guid.Parse("A1F39FFD-1161-4474-8823-8BB441395A2A"), */ FullName = "Fiona Miles",  Roster = cons, StaffMemberCode="M" },
                    new ServerStaffMember { /*Id = Guid.Parse("F4DE1C5A-AA18-4A99-B300-6183000CDC93"), */ FullName = "Gabrielle Nuthall",  Roster = cons, StaffMemberCode="N" },
                    new ServerStaffMember { /*Id = Guid.NewGuid(),*/  FullName = "Dave Buckley",  Roster = cons, StaffMemberCode="BU" },
                    new ServerStaffMember { /*Id = Guid.NewGuid(),*/  FullName = "John Beca",  Roster = cons, StaffMemberCode="BE" },
                    new ServerStaffMember { /*Id = Guid.NewGuid(),*/  FullName = "Brian Anderson",  Roster = cons, StaffMemberCode="A" },
                    new ServerStaffMember { /*Id = Guid.NewGuid(),*/  FullName = "Liz Segedin",  Roster = cons, StaffMemberCode="S" }
                });
            }
        }

        public static byte[] HexadecimalStringToByteArray_Original(string input)
        {
            var outputLength = input.Length / 2;
            var output = new byte[outputLength];
            for (var i = 0; i < outputLength; i++)
            {
                output[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return output;
        }
    }
}

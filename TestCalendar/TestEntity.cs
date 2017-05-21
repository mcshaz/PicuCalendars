using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicuCalendars.DataAccess;
using System.Linq;

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
    }
}

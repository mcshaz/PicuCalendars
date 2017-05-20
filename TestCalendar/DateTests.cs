using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PicuCalendars.Models;

namespace TestCalendar
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void TestShiftModel()
        {
            //check if daylight saving handled properly
            for (int i=1; i <= 12; i++)
            {
                var json = $"{{\"Date\":\"2017-{i:D2}-01\",\"ShiftCode\":\"E1\",\"ShiftInitials\":[\"A\"]}}";
                var sm = JsonConvert.DeserializeObject<Appointment>(json);
                Assert.AreEqual(1, sm.Date.Day);
            }
            
        }
    }
}

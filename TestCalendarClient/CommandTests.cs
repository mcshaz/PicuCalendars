using ExcelRosterReader.CommandLineParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCalendar
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void TestInvalidFiles()
        {
            var tested = new CreateCommand();
            Assert.IsFalse(tested.ExcelFileOk("gobledegook.xlsx", "test"));
            using (var temp = new TempFile("tmp.txt", string.Empty))
            {
                Assert.IsFalse(tested.ExcelFileOk(temp._file.FullName, "test"));
            }
            using (var temp = new TempFile("tmp.xlsx", string.Empty))
            {
                Assert.IsFalse(tested.ExcelFileOk(temp._file.FullName, "test"));
            }
        }
    }

    internal sealed class TempFile : IDisposable
    {
        internal readonly FileInfo _file;
        public TempFile(string name, string text)
        {
            _file = new FileInfo(name);
            using (var sw = _file.CreateText())
            {
                sw.Write(text);
            }
        }
        public void Dispose()
        {
            if (_file != null)
            {
                _file.Delete();
            }
            
        }
    }
}

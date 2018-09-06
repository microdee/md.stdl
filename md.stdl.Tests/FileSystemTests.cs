using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Windows;
using Xunit;

namespace md.stdl.Tests
{
    public class FileSystemTests
    {
        [Fact]
        public void GetRelativePathExistingTest()
        {
            var p0 = @"C:\Windows\System32";
            var p1 = @"C:\Windows\Cursors";

            var pw = @"C:\Windows";
            var pe = @"C:\Windows\explorer.exe";
            
            Assert.Equal(@"..\Cursors", FileSystem.GetRelativePath(p0, p1));
            Assert.Equal(@"..\System32", FileSystem.GetRelativePath(p1, p0));
            Assert.Equal(@".\explorer.exe", FileSystem.GetRelativePath(pw, pe));
        }
    }
}

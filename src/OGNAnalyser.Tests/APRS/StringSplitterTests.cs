using OGNAnalyser.Client.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OGNAnalyser.Tests.APRS
{
    public class StringSplitterTests
    {
        [Fact]
        public void SplitterKeepsLineEndings()
        {
            string input = "test1\r\ntest2\r\n";
            var parts = input.SplitKeepDelim("\r\n").ToArray();
            Assert.Equal("test1\r\n", parts[0]);
            Assert.Equal("test2\r\n", parts[1]);
        }

        [Fact]
        public void SplitterKeepsLineWithoutDelim()
        {
            string input = "test1test2";
            var parts = input.SplitKeepDelim("\r\n").ToArray();
            Assert.Equal("test1test2", parts[0]);
        }

        [Fact]
        public void SplitterKeepsEmptyLinesWithDelim()
        {
            string input = "test1\r\ntest2\r\n\r\n";
            var parts = input.SplitKeepDelim("\r\n").ToArray();
            Assert.Equal("test1\r\n", parts[0]);
            Assert.Equal("test2\r\n", parts[1]);
            Assert.Equal("\r\n", parts[2]);
        }
    }
}

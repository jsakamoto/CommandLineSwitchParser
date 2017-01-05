using System;
using System.Linq;
using Xunit;

namespace CommandLineSwitchParser.Test
{
    public class CommandLineSwitchParserTest
    {
        public class Option1
        {
            public bool Recuruce { get; set; }

            public int Port { get; set; } = 8080;
        }

        [Fact]
        public void EmptyArgs_Test()
        {
            var args = new string[] { };
            var options = CommandLineSwitch.Parse<Option1>(ref args);
            options.Recuruce.Is(false);
            options.Port.Is(8080);
            args.Is();
        }

        [Fact]
        public void ComplexArgs_Test()
        {
            var args = new[] { "--port", "80", @"c:\wwwroot\inetpub", "-r" };
            var options = CommandLineSwitch.Parse<Option1>(ref args);
            options.Recuruce.Is(true);
            options.Port.Is(80);
            args.Is(@"c:\wwwroot\inetpub");
        }
    }
}

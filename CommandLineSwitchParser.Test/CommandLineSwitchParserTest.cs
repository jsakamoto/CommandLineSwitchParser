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

        [Fact(DisplayName = "Parse() - Unknown option")]
        public void Parse_UnknownOption_Test()
        {
            var commandline = "-p 80 -z USWest -r -s";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<Option1>(ref args);
            });

            e.Message.Is("-z is unknown switch/option.");
            e.ParserError.ErrorType.Is(ErrorTypes.UnkdonwOption);
            e.ParserError.OptionName.Is("-z");
            e.ParserError.Parameter.IsNull();
            e.ParserError.ExpectedParameterType.IsNull();

            args.Is(commandline.Split(' '));
        }
    }
}

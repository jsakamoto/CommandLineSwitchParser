using System;
using System.Linq;
using CommandLineSwitchParser.Test.Fixture;
using Xunit;

namespace CommandLineSwitchParser.Test
{
    public class CommandLineSwitchParserTest
    {
        [Fact]
        public void EmptyArgs_Test()
        {
            var args = new string[] { };
            var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            options.Recursive.Is(false);
            options.Port.Is<uint>(8080);
            args.Is();
        }

        [Fact]
        public void ComplexArgs_Test()
        {
            var args = new[] { "--port", "80", @"c:\wwwroot\inetpub", "-r" };
            var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            options.Recursive.Is(true);
            options.Port.Is<uint>(80);
            args.Is(@"c:\wwwroot\inetpub");
        }

        [Fact(DisplayName = "Parse() - Unknown option")]
        public void Parse_UnknownOption_Test()
        {
            var commandline = "-p 80 -z USWest -r -p";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            });

            e.Message.Is("-z is unknown switch/option.");
            e.ParserError.ErrorType.Is(ErrorTypes.UnkdonwOption);
            e.ParserError.OptionName.Is("-z");
            e.ParserError.Parameter.IsNull();
            e.ParserError.ExpectedParameterType.IsNull();

            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "Parse() - Missing parameter")]
        public void Parse_MissingParameter_Test()
        {
            var commandline = "--recursive --port";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            });

            e.Message.Is("The parameter of --port is missing.");
            e.ParserError.ErrorType.Is(ErrorTypes.MissingParameter);
            e.ParserError.OptionName.Is("--port");
            e.ParserError.Parameter.IsNull();
            e.ParserError.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "Parse() - Invalid parameter format - int")]
        public void Parse_InvalidParameterFormat_Int_Test()
        {
            var commandline = "-p http://localhost/ -z";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            });

            e.Message.Is("The parameter of -p is not an unsigned integer.");
            e.ParserError.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            e.ParserError.OptionName.Is("-p");
            e.ParserError.Parameter.Is("http://localhost/");
            e.ParserError.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "Parse() - Invalid parameter format - date/time")]
        public void Parse_InvalidParameterFormat_DateTime_Test()
        {
            var commandline = "-c Today -m hello";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<VCSCommandOptions>(ref args);
            });

            e.Message.Is("The parameter of -c is not a date / time.");
            e.ParserError.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            e.ParserError.OptionName.Is("-c");
            e.ParserError.Parameter.Is("Today");
            e.ParserError.ExpectedParameterType.Is(typeof(DateTime));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "Parse() - Invalid parameter format - decimal")]
        public void Parse_InvalidParameterFormat_Decimal_Test()
        {
            var commandline = "-c 2017-09-14 -r Hello -z";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<VCSCommandOptions>(ref args);
            });

            e.Message.Is("The parameter of -r is not a number.");
            e.ParserError.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            e.ParserError.OptionName.Is("-r");
            e.ParserError.Parameter.Is("Hello");
            e.ParserError.ExpectedParameterType.Is(typeof(decimal));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "Parse() - parameter overlow - int")]
        public void Parse_ParameterOverflow_Int_Test()
        {
            var commandline = "--port -80 -z";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            });

            e.Message.Is("The parameter of --port is too large or too small.");
            e.ParserError.ErrorType.Is(ErrorTypes.ParameterOverflow);
            e.ParserError.OptionName.Is("--port");
            e.ParserError.Parameter.Is("-80");
            e.ParserError.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }
    }
}

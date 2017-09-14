using System;
using CommandLineSwitchParser.Test.Fixture;
using Xunit;

namespace CommandLineSwitchParser.Test
{
    public class TryParseTest
    {
        [Fact(DisplayName = "TryParse() - empty args")]
        public void TryParse_EmptyArgs_Test()
        {
            var args = new string[] { };
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsTrue();
            options.Recursive.Is(false);
            options.Port.Is<uint>(8080);
            args.Is();
            err.IsNull();
        }

        [Fact(DisplayName = "TryParse() - complex args")]
        public void TryParse_ComplexArgs_Test()
        {
            var args = new[] { "--port", "80", @"c:\wwwroot\inetpub", "-r" };
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsTrue();
            options.Recursive.Is(true);
            options.Port.Is<uint>(80);
            args.Is(@"c:\wwwroot\inetpub");
            err.IsNull();
        }

        [Fact(DisplayName = "TryParse() - Unknown option")]
        public void TryParse_UnknownOption_Test()
        {
            var commandline = "-p 80 -z USWest -r -p";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.UnkdonwOption);
            err.OptionName.Is("-z");
            err.Parameter.IsNull();
            err.ExpectedParameterType.IsNull();
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "TryParse() - Missing parameter")]
        public void TryParse_MissingParameter_Test()
        {
            var commandline = "--recursive --port";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.MissingParameter);
            err.OptionName.Is("--port");
            err.Parameter.IsNull();
            err.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "TryParse() - Invalid parameter format - int")]
        public void TryParse_InvalidParameterFormat_Int_Test()
        {
            var commandline = "-p http://localhost/ -z";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            err.OptionName.Is("-p");
            err.Parameter.Is("http://localhost/");
            err.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "TryParse() - Invalid parameter format - date/time")]
        public void TryParse_InvalidParameterFormat_DateTime_Test()
        {
            var commandline = "-c Today -m hello";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<VCSCommandOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            err.OptionName.Is("-c");
            err.Parameter.Is("Today");
            err.ExpectedParameterType.Is(typeof(DateTime));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "TryParse() - Invalid parameter format - decimal")]
        public void TryParse_InvalidParameterFormat_Decimal_Test()
        {
            var commandline = "-c 2017-09-14 -r Hello -z";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<VCSCommandOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            err.OptionName.Is("-r");
            err.Parameter.Is("Hello");
            err.ExpectedParameterType.Is(typeof(decimal));
            args.Is(commandline.Split(' '));
        }

        [Fact(DisplayName = "TryParse() - parameter overlow - int")]
        public void TryParse_ParameterOverflow_Int_Test()
        {
            var commandline = "--port -80 -z";
            var args = commandline.Split(' ');
            CommandLineSwitch.TryParse<HttpServerOptions>(ref args, out var options, out var err).IsFalse();
            options.IsNull();
            err.ErrorType.Is(ErrorTypes.ParameterOverflow);
            err.OptionName.Is("--port");
            err.Parameter.Is("-80");
            err.ExpectedParameterType.Is(typeof(uint));
            args.Is(commandline.Split(' '));
        }
    }
}

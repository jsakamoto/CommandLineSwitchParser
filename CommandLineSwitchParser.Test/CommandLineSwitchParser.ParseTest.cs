using System;
using CommandLineSwitchParser.Test.Fixture;
using Xunit;

namespace CommandLineSwitchParser.Test
{
    public class ParseTest
    {
        [Fact(DisplayName = "Parse() - empty args")]
        public void EmptyArgs_Test()
        {
            var args = new string[] { };
            var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            options.Recursive.Is(false);
            options.Port.Is<uint>(8080);
            args.Is();
        }

        [Fact(DisplayName = "Parse() - complex args")]
        public void ComplexArgs_Test()
        {
            var args = new[] { "--port", "80", @"c:\wwwroot\inetpub", "-r" };
            var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            options.Recursive.Is(true);
            options.Port.Is<uint>(80);
            args.Is(@"c:\wwwroot\inetpub");
        }

        [Fact(DisplayName = "Parse() - enum args")]
        public void Parse_Enum_Test()
        {
            var args = "-t svn commit".Split(' ');
            var options = CommandLineSwitch.Parse<VCSCommandOptions>(ref args);
            options.Type.Is(VCSTypes.SVN);
            args.Is("commit");
        }

        [Theory(DisplayName = "Parse() - enum args with parser style")]
        [InlineData("git", EnumParserStyle.LowerCase)]
        [InlineData("GIT", EnumParserStyle.UpperCase)]
        [InlineData("Git", EnumParserStyle.OriginalCase)]
        [InlineData("Git", EnumParserStyle.OriginalCase | EnumParserStyle.LowerCase)]
        [InlineData("GIT", EnumParserStyle.LowerCase | EnumParserStyle.UpperCase)]
        [InlineData("GIT", EnumParserStyle.OriginalCase | EnumParserStyle.UpperCase)]
        [InlineData("gIt", EnumParserStyle.IgnoreCase)]
        [InlineData("giT", EnumParserStyle.IgnoreCase | EnumParserStyle.LowerCase)]
        [InlineData("GiT", EnumParserStyle.IgnoreCase | EnumParserStyle.UpperCase)]
        [InlineData("gIT", EnumParserStyle.IgnoreCase | EnumParserStyle.OriginalCase)]
        public void Parse_Enum_with_ParserStyle_Test(string enumValueText, EnumParserStyle enumParserStyle)
        {
            var args = new[] { "-t", enumValueText, "commit" };
            var options = CommandLineSwitch.Parse<VCSCommandOptions>(
                ref args,
                parserOptions => parserOptions.EnumParserStyle = enumParserStyle);
            options.Type.Is(VCSTypes.Git);
            args.Is("commit");
        }

        [Fact(DisplayName = "Parse() - long name only")]
        public void Parse_LongNameOnly_Test()
        {
            var args = "http://localhost:32767 --authenticationtype cookie --allowanonymous".Split(' ');
            var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            options.AuthenticationType.Is(AuthenticationType.Cookie);
            options.AllowAnonymous.IsTrue();

        }

        [Fact(DisplayName = "Parse() - long name only - ambiguous error")]
        public void Parse_LongNameOnly_Ambiguous_Test()
        {
            var args = "http://localhost:32767 -a".Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<HttpServerOptions>(ref args);
            });
            e.ParserError.ErrorType.Is(ErrorTypes.UnknownOption);
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
            e.ParserError.ErrorType.Is(ErrorTypes.UnknownOption);
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

        [Fact(DisplayName = "Parse() - Invalid parameter format - enum")]
        public void Parse_InvalidParameterFormat_Enum_Test()
        {
            var commandline = "-c 2017-09-14 --type Git -z";
            var args = commandline.Split(' ');
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<VCSCommandOptions>(ref args);
            });

            e.Message.Is("The parameter of --type is not the one of git, svn.");
            e.ParserError.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            e.ParserError.OptionName.Is("--type");
            e.ParserError.Parameter.Is("Git");
            e.ParserError.ExpectedParameterType.Is(typeof(VCSTypes));
            args.Is(commandline.Split(' '));
        }

        [Theory(DisplayName = "Parse() - Invalid parameter format - enum with parser style")]
        [InlineData("Git", "git, svn", EnumParserStyle.LowerCase)]
        [InlineData("git", "GIT, SVN", EnumParserStyle.UpperCase)]
        [InlineData("git", "Git, SVN", EnumParserStyle.OriginalCase)]
        [InlineData("GIT", "git, Git, svn, SVN", EnumParserStyle.OriginalCase | EnumParserStyle.LowerCase)]
        [InlineData("Git", "git, GIT, svn, SVN", EnumParserStyle.UpperCase | EnumParserStyle.LowerCase)]
        [InlineData("giT", "git, Git, GIT, svn, SVN", EnumParserStyle.OriginalCase | EnumParserStyle.UpperCase | EnumParserStyle.LowerCase)]
        [InlineData("hg", "Git, SVN", EnumParserStyle.IgnoreCase)]
        public void Parse_InvalidParameterFormat_Enum_with_ParserStyle_Test(string enumValueText, string expectedValues, EnumParserStyle enumParserStyle)
        {
            var args = new[] { "--type", enumValueText, "-z" };
            var e = Assert.Throws<InvalidCommandLineSwitchException>(() =>
            {
                var options = CommandLineSwitch.Parse<VCSCommandOptions>(ref args, parserOptions => parserOptions.EnumParserStyle = enumParserStyle);
            });

            e.Message.Is($"The parameter of --type is not the one of {expectedValues}.");
            e.ParserError.ErrorType.Is(ErrorTypes.InvalidParameterFormat);
            e.ParserError.OptionName.Is("--type");
            e.ParserError.Parameter.Is(enumValueText);
            e.ParserError.ExpectedParameterType.Is(typeof(VCSTypes));
            args.Is("--type", enumValueText, "-z");
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

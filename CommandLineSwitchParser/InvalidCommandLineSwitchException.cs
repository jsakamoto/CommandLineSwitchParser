using System;

namespace CommandLineSwitchParser
{
    public class InvalidCommandLineSwitchException : Exception
    {
        public override string Message => this.ParserError.ToString();

        public CommandLineSwitchParserError ParserError { get; }

        internal InvalidCommandLineSwitchException(ErrorTypes errorType, string optionName, string parameter, Type expectedParameterType, CommandLineSwitchParserOptions parserOptions)
            : this(errorType, optionName, parameter, expectedParameterType, null, parserOptions)
        {
        }

        internal InvalidCommandLineSwitchException(ErrorTypes errorType, string optionName, string parameter, Type expectedParameterType, Exception innerException, CommandLineSwitchParserOptions parserOptions)
            : base("", innerException)
        {
            this.ParserError = new CommandLineSwitchParserError(errorType, optionName, parameter, expectedParameterType, parserOptions);
        }

        /*
        memo
        ------
         
        ### Case
        
        - (a) Unkdonw switch/option.
        - (b) Missing option parameter.
        - (c) The option paraeter is invalid format.

        ### Information

        - Message
        - Switch/Option name
        - Option parameter (case (c) only)
        - Expected option parameter type (case (c) only)

        */
    }
}

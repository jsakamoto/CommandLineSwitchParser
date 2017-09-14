using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineSwitchParser
{
    public class InvalidCommandLineSwitchException : Exception
    {
        public override string Message => this.ParserError.ToString();

        public CommandLineSwitchParserError ParserError { get; }

        internal InvalidCommandLineSwitchException(ErrorTypes errorType, string optionName, string parameter, Type expectedParameterType)
        {
            this.ParserError = new CommandLineSwitchParserError(errorType, optionName, parameter, expectedParameterType);
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

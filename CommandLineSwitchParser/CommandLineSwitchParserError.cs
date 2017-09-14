using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineSwitchParser
{
    public class CommandLineSwitchParserError
    {
        public ErrorTypes ErrorType { get; }

        public string OptionName { get; }

        public string Parameter { get; }

        public Type ExpectedParameterType { get; }

        internal CommandLineSwitchParserError(ErrorTypes errorType, string optionName, string parameter, Type expectedParameterType)
        {
            ErrorType = errorType;
            OptionName = optionName;
            Parameter = parameter;
            ExpectedParameterType = expectedParameterType;
        }

        public override string ToString()
        {
            switch (this.ErrorType)
            {
                case ErrorTypes.UnkdonwOption:
                    return $"{this.OptionName} is unknown switch/option.";
                case ErrorTypes.MissingParameter:
                    break;
                case ErrorTypes.InvalidFormatParameter:
                    break;
                default:
                    break;
            }
            return base.ToString();
        }
    }
}

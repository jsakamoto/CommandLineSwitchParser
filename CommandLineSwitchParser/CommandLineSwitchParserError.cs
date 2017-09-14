using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
                    return $"The parameter of {this.OptionName} is missing.";
                case ErrorTypes.InvalidParameterFormat:
                    return $"The parameter of {this.OptionName} is not {this.GetParameterTypeText(this.ExpectedParameterType)}.";
                case ErrorTypes.ParameterOverflow:
                    return $"The parameter of {this.OptionName} is too large or too small.";
                default:
                    break;
            }
            return base.ToString();
        }

        private string GetParameterTypeText(Type expectedParameterType)
        {
            if (expectedParameterType == typeof(DateTime))
                return "a date / time";

            var typeName = expectedParameterType.FullName;
            var matchAsInt = Regex.Match(typeName, @"^System\.(?<u>U)?Int\d+$");
            if (matchAsInt.Success)
            {
                if (matchAsInt.Groups["u"].Success) return "an unsigned integer";
                return "an integer";
            }

            if (Regex.IsMatch(typeName, @"^System\.(Double|Single|Decimal)$")) return "a number";

            return expectedParameterType.Name;
        }
    }
}

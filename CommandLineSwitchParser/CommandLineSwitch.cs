using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandLineSwitchParser
{
    public static class CommandLineSwitch
    {
        public static TOptions Parse<TOptions>(ref string[] args) where TOptions : new()
        {
            return Parse<TOptions>(ref args, _ => { });
        }

        public static TOptions Parse<TOptions>(ref string[] args, Action<CommandLineSwitchParserOptions> configureOptions) where TOptions : new()
        {
            var paserOptions = new CommandLineSwitchParserOptions();
            configureOptions(paserOptions);

            var options = new TOptions();
            var optionDefs = BuildOptionDefs(options);

            var omittedArgs = new List<string>();
            var enumerator = args.Cast<string>().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var arg = enumerator.Current;
                var optDef = default(OptionDef);
                var result = TryFindOptionDef(optionDefs, arg, out optDef);

                if (result == FindOptDefResult.ArgIsNotOption)
                    omittedArgs.Add(arg);

                else if (result == FindOptDefResult.Success)
                {
                    if (optDef.Type == OptionType.Switch)
                    {
                        optDef.PropInfo.SetValue(options, true);
                    }
                    else
                    {
                        var endOfArgs = !enumerator.MoveNext();
                        if (endOfArgs) throw new InvalidCommandLineSwitchException(ErrorTypes.MissingParameter, arg, null, optDef.PropInfo.PropertyType, paserOptions);
                        try
                        {
                            var optionParam = enumerator.Current;
                            var propType = optDef.PropInfo.PropertyType;

                            if (propType.GetTypeInfo().IsEnum)
                            {
                                var enumNames = Enum.GetNames(propType).ToArray();
                                var originalEnumNameToValues = enumNames.ToDictionary(name => name, name => Enum.Parse(propType, name));
                                var lowerCaseEnumNameToValues = originalEnumNameToValues.ToDictionary(item => item.Key.ToLower(), item => item.Value);
                                var upperCaseEnumNameToValues = originalEnumNameToValues.ToDictionary(item => item.Key.ToUpper(), item => item.Value);

                                if (paserOptions.EnumParserStyle.HasFlag(EnumParserStyle.IgnoreCase) && lowerCaseEnumNameToValues.TryGetValue(optionParam.ToLower(), out var convertedValue1))
                                {
                                    optDef.PropInfo.SetValue(options, convertedValue1);
                                }
                                else if (paserOptions.EnumParserStyle.HasFlag(EnumParserStyle.LowerCase) && lowerCaseEnumNameToValues.TryGetValue(optionParam, out var convertedValue2))
                                {
                                    optDef.PropInfo.SetValue(options, convertedValue2);
                                }
                                else if (paserOptions.EnumParserStyle.HasFlag(EnumParserStyle.UpperCase) && upperCaseEnumNameToValues.TryGetValue(optionParam, out var convertedValue3))
                                {
                                    optDef.PropInfo.SetValue(options, convertedValue3);
                                }
                                else if (paserOptions.EnumParserStyle.HasFlag(EnumParserStyle.OriginalCase) && originalEnumNameToValues.TryGetValue(optionParam, out var convertedValue4))
                                {
                                    optDef.PropInfo.SetValue(options, convertedValue4);
                                }
                                else throw new FormatException();
                            }

                            else
                            {
                                var convertedValue = Convert.ChangeType(optionParam, propType);
                                optDef.PropInfo.SetValue(options, convertedValue);
                            }
                        }
                        catch (FormatException e)
                        {
                            throw new InvalidCommandLineSwitchException(ErrorTypes.InvalidParameterFormat, arg, enumerator.Current, optDef.PropInfo.PropertyType, e, paserOptions);
                        }
                        catch (OverflowException e)
                        {
                            throw new InvalidCommandLineSwitchException(ErrorTypes.ParameterOverflow, arg, enumerator.Current, optDef.PropInfo.PropertyType, e, paserOptions);
                        }
                    }
                }
                else if (result == FindOptDefResult.NotFound)
                {
                    throw new InvalidCommandLineSwitchException(ErrorTypes.UnknownOption, arg, null, null, paserOptions);
                }
            }

            args = omittedArgs.ToArray();

            return options;
        }

        public static bool TryParse<TOptions>(ref string[] args, out TOptions options, out CommandLineSwitchParserError err) where TOptions : new()
        {
            err = null;
            options = default(TOptions);
            try
            {
                options = Parse<TOptions>(ref args);
                return true;
            }
            catch (InvalidCommandLineSwitchException e)
            {
                err = e.ParserError;
                return false;
            }
        }

        private static OptionDef[] BuildOptionDefs(object options)
        {
            var optionDefs = options
                .GetType()
                .GetRuntimeProperties()
                .Select(prop => new OptionDef
                {
                    ShortName = prop.Name.ToLower().Substring(0, 1),
                    LongName = prop.Name.ToLower(),
                    Type = prop.PropertyType == typeof(bool) ? OptionType.Switch : OptionType.Parameter,
                    PropInfo = prop
                })
                .ToArray();

            // Clear ambiguous short name.
            var ambiguousDefs = optionDefs
                .GroupBy(d => d.ShortName)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g);
            foreach (var ambiguousDef in ambiguousDefs)
            {
                ambiguousDef.ShortName = null;
            }

            return optionDefs;
        }

        private enum FindOptDefResult
        {
            ArgIsNotOption,
            NotFound,
            Success
        }

        private static FindOptDefResult TryFindOptionDef(IEnumerable<OptionDef> optionDefs, string arg, out OptionDef optDef)
        {
            optDef = null;
            var matchShortOpt = Regex.Match(arg, "^-.$");
            var matchLongOpt = Regex.Match(arg, "^--.+$");

            if (matchShortOpt.Success)
            {
                var optName = arg.Substring(1);
                optDef = optionDefs.FirstOrDefault(def => def.ShortName == optName);
            }
            else if (matchLongOpt.Success)
            {
                var optName = arg.Substring(2);
                optDef = optionDefs.FirstOrDefault(def => def.LongName == optName);
            }
            else return FindOptDefResult.ArgIsNotOption;

            return optDef != null ? FindOptDefResult.Success : FindOptDefResult.NotFound;
        }
    }
}

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
                        if (endOfArgs) throw new InvalidCommandLineSwitchException(ErrorTypes.MissingParameter, arg, null, optDef.PropInfo.PropertyType);
                        try
                        {
                            var optionParam = enumerator.Current;
                            var propType = optDef.PropInfo.PropertyType;

                            if (propType.GetTypeInfo().IsEnum)
                            {
                                var enumNameToValues = Enum.GetNames(propType)
                                    .ToDictionary(name => name.ToLower(), name => Enum.Parse(propType, name));
                                if (enumNameToValues.TryGetValue(optionParam, out var convertedValue))
                                {
                                    optDef.PropInfo.SetValue(options, convertedValue);
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
                            throw new InvalidCommandLineSwitchException(ErrorTypes.InvalidParameterFormat, arg, enumerator.Current, optDef.PropInfo.PropertyType, e);
                        }
                        catch (OverflowException e)
                        {
                            throw new InvalidCommandLineSwitchException(ErrorTypes.ParameterOverflow, arg, enumerator.Current, optDef.PropInfo.PropertyType, e);
                        }
                    }
                }
                else if (result == FindOptDefResult.NotFound)
                {
                    throw new InvalidCommandLineSwitchException(ErrorTypes.UnkdonwOption, arg, null, null);
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

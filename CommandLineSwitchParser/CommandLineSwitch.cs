﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandLineSwitchParser
{
    internal enum OptionType
    {
        Switch,
        Parameter
    }

    internal class OptionDef
    {
        public string ShortName { get; set; }

        public string LongName { get; set; }

        public OptionType Type { get; set; }

        public PropertyInfo PropInfo { get; set; }
    }

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
                        enumerator.MoveNext();
                        var convertedValue = Convert.ChangeType(enumerator.Current, optDef.PropInfo.PropertyType);
                        optDef.PropInfo.SetValue(options, convertedValue);
                    }
                }
            }

            args = omittedArgs.ToArray();

            return options;
        }

        private static OptionDef[] BuildOptionDefs(object options)
        {
            return options
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
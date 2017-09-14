using System;
using System.Reflection;

namespace CommandLineSwitchParser
{
    internal class OptionDef
    {
        public string ShortName { get; set; }

        public string LongName { get; set; }

        public OptionType Type { get; set; }

        public PropertyInfo PropInfo { get; set; }
    }
}

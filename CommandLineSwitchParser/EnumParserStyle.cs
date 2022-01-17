using System;

namespace CommandLineSwitchParser
{
    [Flags]
    public enum EnumParserStyle
    {
        LowerCase = 0x0001,
        OriginalCase = 0x0002,
        UpperCase = 0x004,
        IgnoreCase = 0x0008
    }
}

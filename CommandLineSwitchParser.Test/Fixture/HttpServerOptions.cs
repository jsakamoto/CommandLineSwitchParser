using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineSwitchParser.Test.Fixture
{
    public class HttpServerOptions
    {
        public bool Recursive { get; set; }

        public uint Port { get; set; } = 8080;
    }
}

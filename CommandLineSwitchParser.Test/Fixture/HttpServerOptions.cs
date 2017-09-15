using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineSwitchParser.Test.Fixture
{
    public enum AuthenticationType
    {
        None,
        BarerToken,
        Cookie
    }

    public class HttpServerOptions
    {
        public bool Recursive { get; set; }

        public uint Port { get; set; } = 8080;

        public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.None;

        public bool AllowAnonymous { get; set; }
    }
}

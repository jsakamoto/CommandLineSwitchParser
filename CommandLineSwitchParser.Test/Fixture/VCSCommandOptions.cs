using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineSwitchParser.Test.Fixture
{
    public enum VCSTypes
    {
        Git,
        SVN
    }

    public class VCSCommandOptions
    {
        public VCSTypes Type { get; set; } = VCSTypes.Git;

        public DateTime CommitAt { get; set; }

        public decimal RepositorySizeLimit { get; set; }
    }
}

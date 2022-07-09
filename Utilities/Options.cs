using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineCounter.Utilities
{
    public class Options
    {
        [Option('l', "languages", Required = false, HelpText = "Print available languages.")]
        public bool Languages { get; set; }

        [Option('f', "file", Required = false, HelpText = "Only count one file.")]
        public bool File { get; set; }

        [Option('e', "exclude", Required = false, Default = "", HelpText = "Comma-separated list of file patterns/names to exclude (uses .gitignore grammar)")]
        public string Exclude { get; set; }

        [Value(0, HelpText = "Path in which to count code (single file if file mode).")]
        public string? Path { get; set; }
    }
}

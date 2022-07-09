using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace LineCounter.Models
{
    public class LanguageList
    {
        KeyValuePair<string, string> Languages { get; set; }
    }
    public class Language
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        public bool? Nested { get; set; }
        public bool? Blank { get; set; }
        [JsonProperty("line_comment")]
        public List<string?>? LineComment { get; set; }
        [JsonProperty("multi_line_comments")]
        public List<List<string>>? MultiLineComments { get; set; }
        public List<List<string>>? Quotes { get; set; }
        [JsonProperty("verbatim_quotes")]
        public List<List<string>>? VerbatimQuotes { get; set; }
        public List<string?>? Extensions { get; set; }
        public List<string?>? Filenames { get; set; }
        public List<string?>? Mime { get; set; }
        public List<string?>? Env { get; set; }
        [JsonProperty("important_syntax")]
        public List<string?>? ImportantSyntax { get; set; }
        public List<string?>? Shebangs { get; set; }
    }
}

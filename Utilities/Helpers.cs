using LineCounter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineCounter.Utilities
{
    public static class Helpers
    {
        public static void ErrorOut(string errorMessage)
        {
            Console.Error.WriteLine(errorMessage);
            Environment.Exit(1);
        }

        public static void PrintLanguages(Dictionary<string, Language> languages)
        {
            var langNames = languages.Keys.OrderBy(x => x).ToList();
            foreach (var langName in langNames)
            {
                var lang = languages[langName];
                string listExtensions()
                {
                    if (lang.Extensions != null)
                    {
                        return $" - Extension{(lang.Extensions.Count > 1 ? "s" : "")}: {string.Join(", ", lang.Extensions)}";
                    }
                    else
                    {
                        return " - No Extensions Found";
                    }
                }
                Console.WriteLine($"{lang.Name ?? langName} {listExtensions()}");
            }
        }
    }
}

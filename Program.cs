﻿using CommandLine;
using LineCounter;
using LineCounter.Models;
using LineCounter.Utilities;
using Newtonsoft.Json;
using Ignore;
using System.Text.Json;
using System.Text.RegularExpressions;

Dictionary<string, Language>? languages = null;
try
{
    string fileText = System.IO.File.ReadAllText("./languages.json");
    languages = JsonConvert.DeserializeObject<Dictionary<string, Language>>(fileText);
    if (languages == null)
    {
        throw new ArgumentNullException();
    }
}
catch (Exception _)
{
    Helpers.ErrorOut("Error loading language config file");
}

Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
{
    if (o.Languages)
    {
        Helpers.PrintLanguages(languages);
        Environment.Exit(0);
    }

    var exclude = new List<IgnoreRule>();
    if (o.Exclude.Length > 0)
    {
        var excluded = o.Exclude.Split(',');
        foreach (var term in excluded)
        {
            exclude.Add(new IgnoreRule(term));
        }
    }
    var counter = new Counter(languages, exclude);
    if (o.File)
    {
        var fileStats = counter.GetFileStats(o.Path);
        fileStats.PrintFileStats();
        Environment.Exit(0);
    }
    else
    {
        var stats = counter.GetDirStats(o.Path);
        stats.PrintSummary();
    }


});







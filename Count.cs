using Ignore;
using LineCounter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LineCounter
{
    public class Counter
    {
        private readonly Dictionary<string, Language> _langs;
        private readonly List<IgnoreRule> _exclude;

        public Counter(Dictionary<string, Language> langs, List<IgnoreRule> exclude)
        {
            _langs = langs;
            _exclude = exclude;
        }

        public Summary GetDirStats(string path)
        {
            var dirStats = new Summary() { Path = path };
            int totalLines = 0;
            int totalChars = 0;
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        var dinfo = new DirectoryInfo(subDir);
                        bool goodToGo = true;
                        foreach (var term in _exclude)
                        {
                            if (term.IsMatch(dinfo.FullName))
                            {
                                goodToGo = false;
                            }
                        }
                        if (goodToGo)
                        {
                            queue.Enqueue(subDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                var thisDir = new Dir();
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var thisFile = GetFileStats(file);
                        if (thisFile != null)
                        {
                            thisDir.Files.Add(thisFile);
                        }
                    }
                }
                dirStats.Dirs.Add(thisDir);
            }

            var allFiles = new List<File>();
            dirStats.Dirs.ForEach(d =>
            {
                d.Files.ForEach(f => { allFiles.Add(f); });
            });
            var filteredFiles = allFiles.Where(x => x.LanguageName != null).ToList();
            dirStats.CodeFiles = filteredFiles.Count();
            (int, int) totals = filteredFiles
                 .Aggregate((0, 0), (total, current) =>
            {
                total.Item1 += current.LineCount;
                total.Item2 += current.CharCount;
                return total;
            });
            (dirStats.TotalLines, dirStats.TotalChars) = totals;

            var grouped = filteredFiles.GroupBy(x => x.LanguageName);
            foreach (var grouping in grouped)
            {
                dirStats.LanguageCounts.Add(grouping.Key, grouping.Aggregate((0, 0, 0), (total, current) =>
                {
                    total.Item1 += current.LineCount;
                    total.Item2 += current.CharCount;
                    total.Item3++;
                    return total;
                }));
            }


            return dirStats;
        }

        public File? GetFileStats(string path)
        {
            var file = new File() { Path = path };
            var finfo = new FileInfo(path);
            foreach (var term in _exclude)
            {
                if (term.IsMatch(finfo.FullName))
                {
                    return null;
                }
            }
            file.Extension = finfo.Extension.Substring(finfo.Extension.IndexOf('.') + 1);
            file.Path = finfo.FullName;
            file.Name = finfo.Name;
            var foundLang = _langs.FirstOrDefault(x =>
            {
                if (x.Value.Extensions == null) return false;
                return x.Value.Extensions.Contains(file.Extension);
            });
            if (!foundLang.Equals(default(KeyValuePair<string, Language>)))
            {
                file.Language = foundLang.Value;
                file.LanguageName = foundLang.Value.Name ?? foundLang.Key;
            }

            string[]? lines = null;
            try
            {
                lines = System.IO.File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                return null;
            }
            if (lines == null)
            {
                return null;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                file.LineCount++;
                for (int j = 0; j < lines[i].Length; j++)
                {
                    if (char.IsWhiteSpace(lines[i][j])) continue;
                    file.CharCount++;
                }
            }
            return file;
        }
    }

    public class Summary
    {
        public List<Dir> Dirs { get; set; } = new List<Dir>();
        public string Path { get; set; }
        public int TotalLines { get; set; }
        public int CodeFiles { get; set; }
        public int TotalChars { get; set; }
        public Dictionary<string, (int, int, int)> LanguageCounts { get; set; } = new Dictionary<string, (int, int, int)>();
        public void PrintSummary()
        {
            var orderedLangs = LanguageCounts.OrderByDescending(x => x.Value.Item1).ToList();
            Console.WriteLine("----------------PATH STATS----------------");
            Console.WriteLine($"Path: {Path}");
            Console.WriteLine($"Number of code files found: {CodeFiles}");
            Console.WriteLine($"Total lines of code: {TotalLines}");
            Console.WriteLine($"Total characters: {TotalChars}");
            Console.WriteLine($"-------------LANGUAGE SUMMARY-------------");
            Console.WriteLine($"Most prevalent language: {orderedLangs[0].Key} - {orderedLangs[0].Value.Item1} lines in {orderedLangs[0].Value.Item3} files");
            Console.WriteLine($"-LANG-----------LINES------CHARS-------FILES-");
            foreach (var lang in orderedLangs)
            {
                Console.WriteLine(@$"{lang.Key,-10}{lang.Value.Item1,10}{lang.Value.Item2,10}{lang.Value.Item3,10}");
            }
        }
    }

    public class File
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public Language Language { get; set; }
        public string? LanguageName { get; set; }
        public int LineCount { get; set; }
        public int CharCount { get; set; }
        public void PrintFileStats()
        {
            Console.WriteLine("----------------FILE STATS----------------");
            Console.WriteLine($"Filename: {Name}");
            Console.WriteLine($"Full path: {Path}");
            Console.WriteLine($"Detected programming language: {LanguageName ?? "None detected"}");
            Console.WriteLine($"------------------------------------------");
            Console.WriteLine($"Line count: {LineCount}");
            Console.WriteLine($"Character count: {CharCount}");
            Console.WriteLine($"------------------------------------------");
        }
    }

    public class Dir
    {
        public List<File> Files { get; set; } = new List<File>();
    }

}

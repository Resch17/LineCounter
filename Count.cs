using Ignore;
using LineCounter.Models;

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
            Queue<string> pathsQueue = new Queue<string>();
            pathsQueue.Enqueue(path);
            while (pathsQueue.Count > 0)
            {
                path = pathsQueue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        var dinfo = new DirectoryInfo(subDir);

                        // only add path to queue if it's doesn't meet the 'exclude' criteria
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
                            pathsQueue.Enqueue(subDir);
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

            var codeFiles = new List<File>();
            dirStats.Dirs.ForEach(d =>
            {
                d.Files.ForEach(f =>
                {
                    if (f.LanguageName != null)
                    {
                        codeFiles.Add(f);
                    }
                });
            });

            dirStats.CodeFiles = codeFiles.Count();

            Totals totals = codeFiles
                  .Aggregate<File, Totals>(new Totals(), (total, current) =>
             {
                 total.LineCount += current.LineCount;
                 total.CharCount += current.CharCount;
                 return total;
             });

            dirStats.TotalLines = totals.LineCount;
            dirStats.TotalChars = totals.CharCount;

            var grouped = codeFiles.GroupBy(x => x.LanguageName);
            foreach (var languageGrouping in grouped)
            {
                if (languageGrouping.Key == null)
                {
                    continue;
                }

                dirStats.LanguageCounts.Add(languageGrouping.Key, languageGrouping.Aggregate<File, Totals>(new Totals(), (total, current) =>
                {
                    total.LineCount += current.LineCount;
                    total.CharCount += current.CharCount;
                    total.FileCount++;
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
        public Dictionary<string, Totals> LanguageCounts { get; set; } = new Dictionary<string, Totals>();
        public void PrintSummary()
        {
            var orderedLangs = LanguageCounts.OrderByDescending(x => x.Value.LineCount).ToList();
            Console.WriteLine("----------------PATH STATS----------------");
            Console.WriteLine($"Path: {Path}");
            Console.WriteLine($"Number of code files found: {CodeFiles}");
            Console.WriteLine($"Total lines of code: {TotalLines}");
            Console.WriteLine($"Total characters: {TotalChars}");
            Console.WriteLine($"-------------LANGUAGE SUMMARY-------------");
            Console.WriteLine($"Most prevalent language: {orderedLangs[0].Key} - {orderedLangs[0].Value.LineCount} lines in {orderedLangs[0].Value.FileCount} files");
            Console.WriteLine($"-LANGUAGE-----------------FILES----LINES----CHARS-");
            foreach (var lang in orderedLangs)
            {
                Console.WriteLine(@$"{lang.Key,-20}{lang.Value.FileCount,10}{lang.Value.LineCount,10}{lang.Value.CharCount,10}");
            }
        }
    }

    public class Totals
    {
        public int LineCount { get; set; } = 0;
        public int CharCount { get; set; } = 0;
        public int FileCount { get; set; } = 0;
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

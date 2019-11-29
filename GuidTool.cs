
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

static class GuidTool {
    public static void Main(string[] args) {
        string path = null;
        string pattern = null;
        bool dryRun = false;

        foreach (var arg in args)
            if (arg == "-y" || arg == "--dry-run")
                dryRun = true;
            else if (string.IsNullOrEmpty(pattern))
                pattern = arg;
            else if (string.IsNullOrEmpty(path))
                path = arg;

        if (string.IsNullOrEmpty(pattern))
            Usage();

        foreach (var file in Directory.GetFiles(path, pattern, SearchOption.AllDirectories)) {
            Console.WriteLine($"Generating guid: {file}");
            var text = File.ReadAllText(file);

            if (dryRun)
                continue;

            var guid = Guid.NewGuid().ToString("N");
            text = Regex.Replace(text, "^guid: (.+)$", $"guid: {guid}", RegexOptions.Multiline);

            File.WriteAllText(file, text);
        }
    }

    static void Usage() {
        Console.Error.WriteLine($"Usage: GuidTool [--dry-run] <pattern> [path]");
        Environment.Exit(1);
    }
}


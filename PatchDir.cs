
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

using static System.Console;
using static System.Environment;
using static System.String;
using static System.IO.Directory;
using static System.IO.Path;
using static System.Diagnostics.Process;
using static System.Text.RegularExpressions.Regex;

enum RevisionControl {
    None,
    Subversion,
    Perforce,
    Git
}

enum Operation {
    None,
    Copy,
    Add,
    Remove
}

static class PatchDir {
    static int verbose = 1;
    static bool force;
    static bool dryRun;
    static bool addOnly;
    static RevisionControl revisionControl = RevisionControl.None;

    static List<string> excluding = new List<string> {".*"};
    static List<string> matching = new List<string>();

    static string source;
    static string target;

    static List<(string, string)> commands = new List<(string, string)>();
    static HashSet<string> parentDirectories = new HashSet<string>();

    static int removeCount;
    static int addCount;
    static int copyCount;

    static void Quit(int status) {
        WriteLine("");
        Exit(status);
    }

    static void PrintUsageAndQuit(string msg = null) {
        if (!IsNullOrEmpty(msg))
            Error.WriteLine(msg);

        Error.WriteLine(
            $"USAGE: {Process.GetCurrentProcess().ProcessName} [OPTIONS]" + NewLine +
            "OPTIONS:" + NewLine +
            "  -f, --force        Force operation" + NewLine +
            "  -v, --verbose      Be verbose" + NewLine +
            "  -q, --quiet        Be quiet" + NewLine +
            "  -y, --dry-run      Print commands only" + NewLine +
            "  -a, --add-only     Never remove files" + NewLine +
            "  -r {svn|p4|git}    Use revision control");

        Quit(1);
    }

    static void ParseArgs(string[] args) {
        for (int i = 0, n = args.Length; i < n; ++i) {
            var arg = args[i];

            if (arg == "-f" || arg == "--force")
                force = true;
            else if (arg == "-v" || arg == "--verbose")
                verbose++;
            else if (arg == "-q" || arg == "--quiet")
                verbose--;
            else if (arg == "-y" || arg == "--dry-run")
                dryRun = true;
            else if (arg == "-a" || arg == "--add-only")
                addOnly = true;
            else if (arg.StartsWith("-x")) {
                if (IsNullOrEmpty(arg = arg.Substring(2)))
                    arg = ++i < n ? args[i] : null;

                if (IsNullOrEmpty(arg))
                    PrintUsageAndQuit("-x expected a substring to match");

                excluding.Add(arg);
            } else if (arg.StartsWith("-m")) {
                if (IsNullOrEmpty(arg = arg.Substring(2)))
                    arg = ++i < n ? args[i] : null;

                if (IsNullOrEmpty(arg))
                    PrintUsageAndQuit("-m expected a substring to match");

                matching.Add(arg);
            } else if (arg.StartsWith("-r")) {
                if (IsNullOrEmpty(arg = arg.Substring(2)))
                    arg = ++i < n ? args[i] : null;

                if (arg == "svn")
                    revisionControl = RevisionControl.Subversion;
                else if (arg == "p4")
                    revisionControl = RevisionControl.Perforce;
                else if (arg == "git")
                    revisionControl = RevisionControl.Git;
                else
                    PrintUsageAndQuit("-r expected one of svn, p4 or git");
            } else if (!arg.StartsWith("-")) {
                if (IsNullOrEmpty(source))
                    source = GetFullPath(arg);
                else if (IsNullOrEmpty(target))
                    target = GetFullPath(arg);
                else
                    PrintUsageAndQuit();
            } else
                PrintUsageAndQuit();
        }

        if (IsNullOrEmpty(source) || IsNullOrEmpty(target))
            PrintUsageAndQuit();
    }

    static void Main(string[] args) {
        ParseArgs(args);

        if (revisionControl == RevisionControl.None)
            if (Exists(".svn"))
                revisionControl = RevisionControl.Subversion;
            else if (Exists(".git"))
                revisionControl = RevisionControl.Git;

        Diff((operation, src, dst) => {
                if (operation == Operation.Remove)
                    RemoveFile(dst);
                else if (operation == Operation.Add)
                    AddFile(src, dst);
                else if (operation == Operation.Copy)
                    CopyFile(src, dst);
            });

        RunCommands();
        WriteLine($"Added {addCount}, removed {removeCount}, copied {copyCount}");
    }

    static void RunCommands() {
        foreach (var command in commands)
            Run(command, dryRun, onLine: null);
    }

    static void RemoveFile(string dst) {
        if (addOnly || matching.Count() > 0 && !matching.Any(x => dst.Contains(x))) {
            if (verbose > 1)
                WriteLine($"Ignoring {dst}");
            return;
        }

        switch (revisionControl) {
        case RevisionControl.Perforce:
            dst = dst
                .Replace("%", "%25")
                .Replace("#", "%23")
                .Replace("*", "%2A")
                .Replace("@", "%40");

            commands.Add(("p4", $"delete -c default \"{dst}\""));
            break;

        case RevisionControl.Subversion:
            commands.Add(("svn", $"remove \"{dst}@\""));
            break;

        case RevisionControl.Git:
            commands.Add(("git", $"remove \"{dst}\""));
            break;

        default:
            commands.Add(("rm", $"-f \"{dst}\""));
            break;
        }

        removeCount++;
    }

    static void AddParentDirectory(string path) {
        var parent = GetDirectoryName(path);

        if (parent.StartsWith(target))
            if (!Exists(parent)) {
                AddParentDirectory(parent);
                commands.Add(("mkdir", $"-p \"{parent}\""));

                if (revisionControl == RevisionControl.Subversion)
                    if (!parentDirectories.Contains(parent)) {
                        parentDirectories.Add(parent);
                        commands.Add(("svn", $"add \"{parent}\""));
                    }
            }
    }

    static void AddFile(string src, string dst) {
        AddParentDirectory(dst);

        commands.Add(("cp", (force ? "-f " : "") + $"\"{src}\" \"{dst}\""));

        switch (revisionControl) {
        case RevisionControl.Perforce:
            commands.Add(("p4", $"add -d -f -c default \"{dst}\""));
            break;

        case RevisionControl.Subversion:
            commands.Add(("svn", $"add \"{dst}@\""));
            break;

        case RevisionControl.Git:
            commands.Add(("git", $"add \"{dst}\""));
            break;
        }

        addCount++;
    }

    static void CopyFile(string src, string dst) {
        if (addOnly || matching.Count() > 0 && !matching.Any(x => dst.Contains(x))) {
            if (verbose > 1)
                WriteLine($"Ignoring {dst}");
            return;
        }

        AddParentDirectory(dst);

        if (revisionControl == RevisionControl.Perforce)
            commands.Add(("p4", $"edit -c default \"{dst}\""));

        commands.Add(("cp", (force ? "-f " : "") + $"\"{src}\" \"{dst}\""));

        if (revisionControl == RevisionControl.Git)
            commands.Add(("git", $"add \"{dst}\""));

        copyCount++;
    }

    static void Diff(Action<Operation, string, string> onFile) {
        var args = "-r" + (verbose < 4 ? " -q" : " -u") +
            excluding.Select(a => $" -x{a}").Aggregate((a, b) => $"{a} {b}") +
            $" '{source}' '{target}'";

        Run(("diff", args), dryRun: false, line => {
                if (verbose > 2)
                    WriteLine(line);

                (string, Operation) GetPathAndOperation() {
                    bool MatchOne(string input, string pattern, out string match) {
                        if (Regex.Match(input, pattern) is Match m && m.Success) {
                            match = m.Groups[1].Value;
                            return true;
                        } else {
                            match = null;
                            return false;
                        }
                    }

                    bool MatchTwo(string input, string pattern, out string match) {
                        if (Regex.Match(input, pattern) is Match m && m.Success) {
                            match = Combine(m.Groups[1].Value, m.Groups[2].Value);
                            return true;
                        } else {
                            match = null;
                            return false;
                        }
                    }

                    string tmp = null;
                    var op = Operation.None;

                    if (MatchOne(line, "^Files (.+) and .*$", out tmp))
                        op = Operation.Copy;
                    else if (MatchTwo(line, "^Only in (.+): (.+)$", out tmp)) {
                        if (tmp.StartsWith(source))
                            op = Operation.Add;
                        else if (tmp.StartsWith(target))
                            op = Operation.Remove;
                    }

                    return (op != Operation.None ? tmp : null, op);
                }

                void ProcessPath((string path, Operation op) arg) {
                    bool isDir = Exists(arg.path);
                    var files = isDir ? GetFiles(arg.path) : new[] {arg.path};

                    foreach (var file in files) {
                        var src = (target != "." ? $"{target}/" : "") + file.Substring(source.Length + 1);
                        var dst = file;

                        if (file.StartsWith(source)) {
                            dst = src;
                            src = file;
                        }

                        onFile(arg.op, src, dst);
                    }

                    if (isDir) {
                        foreach (var dir in GetDirectories(arg.path))
                            ProcessPath((dir, arg.op));

                        if (arg.op == Operation.Remove)
                            RemoveFile(arg.path);
                    }
                }

                ProcessPath(GetPathAndOperation());
            });
    }

    static void Run(ValueTuple<string, string> cmd, bool dryRun, Action<string> onLine) {
        if (verbose > 0)
            WriteLine($"{cmd.Item1} {cmd.Item2}");

        if (dryRun)
            return;

        var proc = Start(new ProcessStartInfo {
                UseShellExecute = false,
                FileName = cmd.Item1,
                Arguments = cmd.Item2,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

        proc.ErrorDataReceived += (sender, e) => Error.WriteLine(e.Data);
        proc.BeginErrorReadLine();

        while (proc.StandardOutput.ReadLine() is string line)
            if (onLine != null)
                onLine(line);
            else
                WriteLine(line);

        proc.WaitForExit();

        if (proc.ExitCode != 0 && cmd.Item1 != "diff") {
            WriteLine($"{cmd.Item1} exited: {proc.ExitCode}");
            Quit(1);
        }
    }
}


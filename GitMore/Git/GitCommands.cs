using envDTE = EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitMore.Git
{
    public static class GitCommands
    {
        public static string GitFetchPruneCommand = "fetch --prune ";

        public static string GitGetLocalBranchCommand = "branch ";
        public static string GitGetRemoteBranchCommand = "branch -r ";

        public static string GitDeleteLocalBranchCommand = "branch -d ";
        public static string GitForceDeleteLocalBranchCommand = "branch -D ";

        public static string GitDeleteRemoteBranchCommand = "push origin --delete ";

        public static string VS_WHERE_PATH = $@"\Microsoft Visual Studio\Installer";

        private static ProcessStartInfo CreateStartInfo(string command, string arguments, string workingDir, Encoding encoding = null)
        {
            return new ProcessStartInfo
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = encoding,
                StandardErrorEncoding = encoding,
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDir
            };
        }

        internal static string GetGitDeleteRemoteBranchCommand(string remoteRoot)
        {
            return $"push {remoteRoot} --delete ";
        }

        public static string GetGitExePath()
        {
            var processOutput = GitCommands.RunVsWhereEx("-property installationpath");
            string[] branches = processOutput.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string currentPath = branches.First();
            return $@"{currentPath}\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\mingw32\bin\git.exe";
        }

        public static string GetGitRepoPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            envDTE.DTE dte = Package.GetGlobalService(typeof(SDTE)) as envDTE.DTE;
            var folderPath = Path.GetDirectoryName(dte.Solution.FullName);
            return FindGitWorkingDir(folderPath);
        }

        public static Process RunGitEx(string command)
        {
            string path = GetGitExePath();

            ThreadHelper.ThrowIfNotOnUIThread();
            string workDir = GetGitRepoPath();
            ProcessStartInfo startInfo = CreateStartInfo(path, command, workDir, Encoding.UTF8);

            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                if (!File.Exists(Path.Combine(path, "Git.exe")))
                {
                    MessageBox.Show("This extension requires Git to be installed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
            }
        }

        public static Process RunVSWhereExe(string command)
        {
            string workDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) + @"\Microsoft Visual Studio\Installer";
            string path = $@"{workDir}\vswhere.exe";
            ProcessStartInfo startInfo = CreateStartInfo(path, command, workDir, Encoding.UTF8);

            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                if (!File.Exists(Path.Combine(path, "vswhere.exe")))
                {
                    MessageBox.Show("This extension requires vswhere to be installed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
            }
        }

        private static string RunVsWhereEx(string command)
        {
            using (var process = RunVSWhereExe(command))
            {
                string output = process.StandardOutput.ReadToEnd();
                int exitCode = process.ExitCode;
                if (exitCode != 0)
                    output = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        public static string RunGitExWait(string command)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            using (var process = RunGitEx(command))
            {
                string output = process.StandardOutput.ReadToEnd();
                int exitCode = process.ExitCode;
                if (exitCode != 0)
                    output = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private static string RunGit(string arguments, string filename, out int exitCode)
        {
            string gitcommand = string.Empty;// GetGitExRegValue("gitcommand");

            ProcessStartInfo startInfo = CreateStartInfo(gitcommand, arguments, filename);

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                exitCode = process.ExitCode;
                process.WaitForExit();
                return output;
            }
        }

        public static string GetCurrentBranch(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string head;
                    string headFileName = FindGitWorkingDir(fileName) + ".git\\HEAD";
                    if (File.Exists(headFileName))
                    {
                        head = File.ReadAllText(headFileName);
                        if (!head.Contains("ref:"))
                        {
                            head = "no branch";
                        }
                    }
                    else
                    {
                        head = RunGit("symbolic-ref HEAD", new FileInfo(fileName).DirectoryName, out var exitCode);
                        if (exitCode == 1)
                        {
                            head = "no branch";
                        }
                    }

                    if (!string.IsNullOrEmpty(head))
                    {
                        head = head.Replace("ref:", "").Trim().Replace("refs/heads/", string.Empty);
                        return head;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        private static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
            {
                return "";
            }

            if (!startDir.EndsWith("\\") && !startDir.EndsWith("/"))
            {
                startDir += "\\";
            }

            var dir = startDir;

            while (dir.LastIndexOfAny(new[] { '\\', '/' }) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new[] { '\\', '/' }));

                if (ValidWorkingDir(dir))
                {
                    return dir + "\\";
                }
            }

            return startDir;
        }

        private static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return false;
            }

            if (Directory.Exists(Path.Combine(dir, ".git")) || File.Exists(Path.Combine(dir, ".git")))
            {
                return true;
            }

            return !dir.Contains(".git") &&
                   Directory.Exists(Path.Combine(dir, "info")) &&
                   Directory.Exists(Path.Combine(dir, "objects")) &&
                   Directory.Exists(Path.Combine(dir, "refs"));
        }
    }
}

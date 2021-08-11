using GitMore.Git;
using GitMore.Model;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GitMore.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class GitCleanManager
    {
        public static ObservableCollection<GitBranch> GetBranches(BranchType type)
        {
            // set git command 
            string gitCommand;
            if (type == BranchType.Remote)
                gitCommand = GitCommands.GitGetRemoteBranchCommand;
            else
                gitCommand = GitCommands.GitGetLocalBranchCommand;

            ThreadHelper.ThrowIfNotOnUIThread();
            return ExtractBranches(type, gitCommand);
        }

        public static ObservableCollection<GitBranch> FetchPruneBranches(BranchType type)
        {

            ThreadHelper.ThrowIfNotOnUIThread();

            string gitCommand = GitCommands.GitFetchPruneCommand;
            return ExtractBranches(type, gitCommand);
        }

        public static string DeleteBranch(GitBranch branch, bool forceDeleteLocal = false)
        {
            string gitCommand;
            string branchName;
            if (branch.Type == BranchType.Remote)
            {
                gitCommand = GitCommands.GetGitDeleteRemoteBranchCommand(branch.RemoteRoot);
                branchName = branch.RemoteName;
            }
            else
            {
                if (forceDeleteLocal)
                    gitCommand = GitCommands.GitForceDeleteLocalBranchCommand;
                else
                    gitCommand = GitCommands.GitDeleteLocalBranchCommand;

                branchName = branch.FullName;
            }


            ThreadHelper.ThrowIfNotOnUIThread();
            return GitCommands.RunGitExWait($"{gitCommand} {branchName}");
        }

        #region Private

        private static ObservableCollection<GitBranch> ExtractBranches(BranchType type, string gitCommand)
        {

            ThreadHelper.ThrowIfNotOnUIThread();

            var branchesCollection = new ObservableCollection<GitBranch>();
            string branchesCommand = GitCommands.RunGitExWait(gitCommand);
            string[] branches = branchesCommand.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (branches?.Length > 0)
            {
                int i = 1;
                foreach (var branch in branches)
                {
                    if (string.IsNullOrWhiteSpace(branch))
                        continue;

                    string branchData = branch.Replace("/", " | ");
                    branchesCollection.Add(
                        new GitBranch
                        {
                            Id = i++,
                            Type = type,
                            DisplayName = $"{branchData?.ToString().Trim()}",
                            FullName = branch?.Trim(),
                            Name = branch.Split('/').Last(),
                            RemoteRoot = branch.Split('/').First().Trim(),
                            RemoteName = string.Join("/", branch.Split('/').SkipWhile(name => name.Trim().Equals("origin")))
                        });
                }
            }
            return branchesCollection;
        }

        #endregion
    }
}

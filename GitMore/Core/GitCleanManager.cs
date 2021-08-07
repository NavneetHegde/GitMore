using GitMore.Git;
using GitMore.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
            string gitCommand = string.Empty;
            if (type == BranchType.Remote)
                gitCommand = GitCommands.GitGetRemoteBranchCommand;
            else
                gitCommand = GitCommands.GitGetLocalBranchCommand;

            return ExtractBranches(type, gitCommand);
        }

        public static ObservableCollection<GitBranch> FetchBranches(BranchType type)
        {
            string gitCommand = GitCommands.GitFetchCommand;
            return ExtractBranches(type, gitCommand);
        }

        public static string DeleteBranch(GitBranch branch)
        {
            string gitCommand;
            string branchName;
            if (branch.Type == BranchType.Remote)
            {
                gitCommand = GitCommands.GitDeleteRemoteBranchCommand;
                branchName = branch.RemoteName;
            }
            else
            {
                gitCommand = GitCommands.GitDeleteLocalBranchCommand;
                branchName = branch.FullName;
            }

            return GitCommands.RunGitExWait($"{gitCommand} {branchName}");
        }

        #region Private

        private static ObservableCollection<GitBranch> ExtractBranches(BranchType type, string gitCommand)
        {
            var branchesCollection = new ObservableCollection<GitBranch>();
            string beanchesCommand = GitCommands.RunGitExWait(gitCommand);
            string[] branches = beanchesCommand.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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
                            RemoteName = string.Join("/", branch.Split('/').SkipWhile(name => name.Trim().Equals("origin")))
                        });
                }
            }
            return branchesCollection;
        }

        #endregion
    }
}

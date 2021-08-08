using GitMore.Core;
using GitMore.Model;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GitMore
{
    /// <summary>
    /// Interaction logic for GitCleanControl.
    /// </summary>
    public partial class GitCleanControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCleanControl"/> class.
        /// </summary>
        public GitCleanControl()
        {
            this.InitializeComponent();
        }

        private void PopulateBranches(BranchType type)
        {
            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"Fetching {type} branches" });

            var BranchesData = new ObservableCollection<GitBranch>();
            BranchesData.Clear();

            ThreadHelper.ThrowIfNotOnUIThread();

            var branches = GitCleanManager.GetBranches(type);
            BranchesData = branches;

            LogData.Add(new LogInfo { Record = $"Fetched total {branches?.Count} {type} branches" });
            ListViewBranches.DataContext = BranchesData;
        }

        private void DeleteBranch(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (Button)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.DataContext;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"Deleting {branchData.Type} branch :: {branchData.FullName}" });

            string deleteCommandResult = GitCleanManager.DeleteBranch(branchData);

            LogData.Add(new LogInfo { Record = $"Deleted {branchData.Type} branch :: {deleteCommandResult}" });

            PopulateBranches(branchData.Type);
        }
    }

}
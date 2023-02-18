using GitMore.Core;
using GitMore.Model;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GitMore
{
    /// <summary>
    /// Interaction logic for GitMoreControl.
    /// </summary>
    public partial class GitMoreControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitMoreControl"/> class.
        /// </summary>
        public GitMoreControl()
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

            var branches = GitMoreManager.GetBranches(type);
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
            LogData.Add(new LogInfo { Record = $"Deleting {branchData.Type} branch begin :: {branchData.FullName}" });

            string deleteCommandResult = GitMoreManager.DeleteBranch(branchData);

            LogData.Add(new LogInfo { Record = $"Deleted {branchData.Type} branch end :: {deleteCommandResult}" });

            PopulateBranches(branchData.Type);
        }

        private void ForceDeleteBranch(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (Button)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.DataContext;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"Force Deleting {branchData.Type} branch begin :: {branchData.FullName}" });

            string deleteCommandResult = GitMoreManager.DeleteBranch(branchData, true);

            LogData.Add(new LogInfo { Record = $"Force Deleted {branchData.Type} branch end :: {deleteCommandResult}" });

            PopulateBranches(branchData.Type);
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (MenuItem)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.CommandParameter;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"Checkout {branchData.Type} branch begin :: {branchData.FullName}" });

            string checkoutCommandResult = GitMoreManager.CheckoutBranch(branchData);


            LogData.Add(new LogInfo { Record = $"Checkout {branchData.Type} branch end :: {checkoutCommandResult}" });

            PopulateBranches(branchData.Type);

        }
    }

}
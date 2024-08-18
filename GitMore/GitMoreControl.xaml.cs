using GitMore.Core;
using GitMore.Model;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
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
            LogData.Add(new LogInfo { Record = $"==== Deleting {branchData.Type} branch begin :: {branchData.FullName} =====" });

            string commandResult = GitMoreManager.DeleteBranch(branchData);

            LogData.Add(new LogInfo { Record = commandResult });
            LogData.Add(new LogInfo { Record = $"=============" });

            PopulateBranches(branchData.Type);
        }

        private void ForceDeleteBranch(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (Button)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.DataContext;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"=== Force Deleting {branchData.Type} branch begin :: {branchData.FullName} =====" });
            
            string commandResult = GitMoreManager.DeleteBranch(branchData, true);

            LogData.Add(new LogInfo { Record = commandResult });
            LogData.Add(new LogInfo { Record = $"=============" });

            PopulateBranches(branchData.Type);
        }

        private void CheckoutMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (MenuItem)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.CommandParameter;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"===== Checkout {branchData.Type} branch begin :: {branchData.FullName} =====" });

            string commandResult = GitMoreManager.CheckoutBranch(branchData);

            LogData.Add(new LogInfo { Record = commandResult });
            LogData.Add(new LogInfo { Record = $"=============" });

            PopulateBranches(branchData.Type);
        }

        private void ViewHistoryMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var butoonContext = (MenuItem)e.OriginalSource;
            GitBranch branchData = (GitBranch)butoonContext.CommandParameter;

            ThreadHelper.ThrowIfNotOnUIThread();

            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Add(new LogInfo { Record = $"===== View hoistory of {branchData.Type} branch begin :: {branchData.FullName} =====" });

            string commandResult = GitMoreManager.ViewHistoryBranch(branchData);

            LogData.Add(new LogInfo { Record = commandResult });
            LogData.Add(new LogInfo { Record = $"=============" });
        }

        private void GoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string commandString = inputTextBox.Text;
            if (!string.IsNullOrWhiteSpace(commandString))
            {
                var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
                LogData.Add(new LogInfo { Record = $"===== Execute command : {commandString} =====" });

                string commandResult = GitMoreManager.ExecuteCustomCommand(commandString);

                LogData.Add(new LogInfo { Record = commandResult });
                LogData.Add(new LogInfo { Record = $"=============" });
            }
        }

        private void Clear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clear log
            var LogData = (ObservableCollection<LogInfo>)ListViewLog.DataContext;
            LogData.Clear();
        }
    }

}
using GitMore.Core;
using GitMore.Git;
using GitMore.Model;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace GitMore
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GitCleanCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        public const string guidGitMorePackageCmdSet = "0b7b7b52-23d2-44eb-8b4b-174873e3b0de";
        public const uint cmdidWindowsMedia = 0x100;
        public const int cmdidWindowsLocalBranch = 0x132;
        public const int cmdidWindowsRemoteBranch = 0x134;
        public const int cmdidWindowsFetchBranch = 0x136;
        public const int ToolbarID = 0x1000;

        public ObservableCollection<GitBranch> BranchesData { get; set; }
        public ObservableCollection<LogInfo> LogData { get; set; }

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0b7b7b52-23d2-44eb-8b4b-174873e3b0de");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitCleanCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GitCleanCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.Execute, menuCommandID);
                commandService.AddCommand(menuItem);

                var toolbarLocalBranchCmdID = new CommandID(new Guid(guidGitMorePackageCmdSet), GitCleanCommand.cmdidWindowsLocalBranch);
                var menuItemLocalBranch = new MenuCommand(new EventHandler(LocalBranchButtonHandler), toolbarLocalBranchCmdID);
                commandService.AddCommand(menuItemLocalBranch);

                var toolbarRemoteBranchCmdID = new CommandID(new Guid(guidGitMorePackageCmdSet), GitCleanCommand.cmdidWindowsRemoteBranch);
                var menuItemRemoteBranch = new MenuCommand(new EventHandler(RemoteBranchButtonHandler), toolbarRemoteBranchCmdID);
                commandService.AddCommand(menuItemRemoteBranch);

                CommandID toolbarFetchBranch = new CommandID(new Guid(guidGitMorePackageCmdSet), GitCleanCommand.cmdidWindowsFetchBranch);
                var menuItemFetchBranch = new MenuCommand(new EventHandler(FetchBranchButtonHandler), toolbarFetchBranch);
                commandService.AddCommand(menuItemFetchBranch);
            }
            BranchesData = new ObservableCollection<GitBranch>();
            LogData = new ObservableCollection<LogInfo>();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GitCleanCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in GitCleanCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GitCleanCommand(package, commandService);

        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(GitClean), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

        }

        private void FetchBranchButtonHandler(object sender, EventArgs e)
        {
            LogData.Add(new LogInfo { Record = $"Fetching remote branches" });

            var branches = GitCleanManager.FetchBranches(BranchType.Remote);

            LogData.Add(new LogInfo { Record = $"Fetched total {branches?.Count} reamote branches" });

            UpdateList();
        }

        private void RemoteBranchButtonHandler(object sender, EventArgs e)
        {
            LogData.Add(new LogInfo { Record = $"Fetching remote branches" });

            var branches = GitCleanManager.GetBranches(BranchType.Remote);
            BranchesData = branches;

            LogData.Add(new LogInfo { Record = $"Fetched total {branches?.Count} reamote branches" });

            UpdateList();
        }


        private void LocalBranchButtonHandler(object sender, EventArgs e)
        {
            LogData.Add(new LogInfo { Record = $"Fetching local branches" });

            var branches = GitCleanManager.GetBranches(BranchType.Local);
            BranchesData = branches;

            LogData.Add(new LogInfo { Record = $"Fetched total {branches?.Count} local branches" });

            UpdateList();
        }

        private void UpdateList()
        {
            ToolWindowPane window = this.package.FindToolWindow(typeof(GitClean), 0, true);

            GitCleanControl control = (GitCleanControl)window.Content;
            control.ListViewBranches.DataContext = BranchesData;
            control.ListViewLog.DataContext = LogData;
        }
    }


}

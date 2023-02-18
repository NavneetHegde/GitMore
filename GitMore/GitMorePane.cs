using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace GitMore
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("720cda5b-8e65-4b64-985f-d038b859344d")]
    public class GitMorePane : ToolWindowPane
    {
        public GitMoreControl control;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitMorePane"/> class.
        /// </summary>
        public GitMorePane() : base(null)
        {
            this.Caption = "Git More";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GitMoreControl();
            this.ToolBar = new CommandID(new Guid(GitMoreCommand.guidGitMorePackageCmdSet), GitMoreCommand.ToolbarID);
            this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }
}

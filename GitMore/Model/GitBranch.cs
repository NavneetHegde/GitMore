namespace GitMore.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class GitBranch
    {
        public int Id { get; set; }
        public BranchType Type { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string RemoteRoot { get; set; }
        public string RemoteName { get; set; }
    }

    public enum BranchType
    {
        Local,
        Remote
    }
}

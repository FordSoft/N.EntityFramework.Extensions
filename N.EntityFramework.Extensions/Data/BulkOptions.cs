namespace N.EntityFramework.Extensions
{
    public class BulkOptions
    {
        public int BatchSize { get; set; }
        public bool UsePermanentTable { get; set; }
        public string TableName { get; set; }
        public int? CommandTimeout { get; set; }
        public string[] IgnoreColumns { get; set; }
        public string PkColumnName { get; set; }
    }
}

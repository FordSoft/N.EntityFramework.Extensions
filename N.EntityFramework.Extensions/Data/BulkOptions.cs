using System.Collections.Generic;

namespace N.EntityFramework.Extensions
{
    public class BulkOptions
    {
        public BulkOptions()
        {
            IgnoreColumns = new List<string>();
        }

        public int BatchSize { get; set; }
        
        public bool UsePermanentTable { get; set; }
        
        public string TableName { get; set; }
        
        public int? CommandTimeout { get; set; }
        
        public List<string> IgnoreColumns { get; set; }
        
        public string PkColumnName { get; set; }

        public string OperationId { get; set; }
    }
}

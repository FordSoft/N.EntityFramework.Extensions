﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.EntityFramework.Extensions
{
    public class BulkOptions
    {
        public int BatchSize { get; set; }
        public bool UsePermanentTable { get; set; }
        public string TableName { get; set; }
        public int? CommandTimeout { get; set; }
        public string[] IgnoreColumns { get; set; }
    }
}

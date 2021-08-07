using System;
using System.Collections.Generic;

namespace N.EntityFramework.Extensions
{
    internal class StringIgnoreCaseEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
            => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(string obj)
            => obj.GetHashCode();
    }
}


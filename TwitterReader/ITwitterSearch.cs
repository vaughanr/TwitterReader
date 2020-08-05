using System;
using System.Collections.Generic;

namespace TwitterReader
{
    public interface ITwitterSearch
    {
        IAsyncEnumerable<TwitterStatus[]> Search(string searchTerm, DateTime toDate);
    }
}

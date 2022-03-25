﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWebApp.Utilities.Pagination
{
    public class PaginatorHelper<T>
    {
        public T PageItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int NumberOfPages { get; set; }
        public int PreviousPage { get; set; }
        public int TotalFormCount { get; set; } = 0;
    }
}

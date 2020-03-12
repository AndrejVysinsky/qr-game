using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class PaginationBase
    {
        public int CurrentPage { get; set; } = 1;
        public int PageLength { get; set; } = 10;
        public int PageCount { get; set; }
    }
}

using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class PaginationViewModel<T> where T: class
    {
        public List<T> Entities { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageLength { get; set; } = 5;
        public int PageCount { get; set; }
    }
}

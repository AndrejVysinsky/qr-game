using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class PaginationViewModel<T> : PaginationBase
    {
        public List<T> Entities { get; set; }        
    }
}

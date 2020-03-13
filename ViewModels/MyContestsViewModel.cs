using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class MyContestsViewModel
    {
        public List<Contest> Contests { get; set; }
        public List<int> AnswersCount { get; set; }
        public List<int> CorrectAnswersCount { get; set; }
    }
}

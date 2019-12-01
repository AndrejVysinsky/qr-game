using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class UserViewModel
    {
        public List<Contest> contests { get; set; }
        public List<int> answersCount { get; set; }
        public List<int> correctAnswersCount { get; set; }
    }
}

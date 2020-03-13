using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class AnswersViewModel
    {
        public PaginationViewModel<ContestQuestionUser> PaginationViewModel { get; set; } = new PaginationViewModel<ContestQuestionUser>();
        public List<string> Users { get; set; }
        public List<string> Contests { get; set; }
        public int SelectedUser { get; set; } = 0;
        public int SelectedContest { get; set; } = 0;
    }
}

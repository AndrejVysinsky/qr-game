using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class AnswersViewModel
    {
        public List<ContestQuestionUser> ContestQuestionUsers { get; set; }
        public List<ApplicationUser> UserList { get; set; }
        public string SelectedUserId { get; set; }
        public List<Contest> ContestList { get; set; }
        public int SelectedContestId { get; set; }
    }
}

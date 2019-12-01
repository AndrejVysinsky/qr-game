using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class AnswersViewModel
    {
        public List<ContestQuestionUser> contestQuestionUsers { get; set; }
        public List<ApplicationUser> userList { get; set; }
        public string selectedUserId { get; set; }
        public List<Contest> contestList { get; set; }
        public int selectedContestId { get; set; }
    }
}

using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class QuestionDeleteViewModel
    {
        public string QuestionName { get; set; }
        public string QuestionText { get; set; }
        public List<ContestQuestion> ContestQuestions { get; set; }
    }
}

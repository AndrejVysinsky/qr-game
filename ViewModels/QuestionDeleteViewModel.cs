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

        //sutaze, ktore obsahuju danu otazku
        public List<string> ContestNames { get; set; }
    }
}

using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class ContestViewModel
    {
        public int ContestId { get; set; }

        [Display(Name = "Názov súťaže")]
        public string ContestName { get; set; }

        //true hodnota značí že otázka už má záznam o odpovedi -> teda nebude možné ju vymazať
        public bool Warning { get; set; }
        public List<KeyValuePair<Question, bool>> SelectedQuestions { get; set; }

        public string SelectedQuestion { get; set; }
        
        [Display(Name = "Otázky")]
        public List<Question> Questions { get; set; }
    }
}

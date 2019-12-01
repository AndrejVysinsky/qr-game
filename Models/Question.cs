using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizWebApp.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Display(Name = "Označenie otázky")]
        public string Name { get; set; }

        [Display(Name = "Text otázky")]
        public string Text { get; set; }

        [Display(Name = "Obrázok k otázke")]
        public string Image { get; set; }

        [Display(Name = "Odpovede")]
        public virtual List<Answer> Answers { get; set; }

        public virtual ICollection<ContestQuestion> ContestQuestions { get; set; }
    }
}

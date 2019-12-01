using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizWebApp.Models
{
    public class Contest
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Otázky súťaže")]
        public virtual List<ContestQuestion> ContestQuestions { get; set; }

        public bool isActive { get; set; }
    }
}


using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuizWebApp.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Display(Name = "Text odpovede")]
        public string Text { get; set; }

        [Display(Name = "Obrázok k odpovedi")]
        public string Image { get; set; }
        public bool IsCorrect { get; set; }
    }
}

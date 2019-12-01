using System;

namespace QuizWebApp.Models
{
    public class ContestQuestionUser
    {
        public string UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public Guid ContestQuestionId { get; set; }
        public virtual ContestQuestion ContestQuestion { get; set; }

        public bool IsAnsweredCorrectly { get; set; }
    }
}

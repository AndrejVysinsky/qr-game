using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.Models
{
    public class ContestQuestion
    {
        public Guid Id { get; set; }

        public int QuestionNumber { get; set; }

        public int ContestId { get; set; }
        public virtual Contest Contest { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public virtual List<ContestQuestionUser> ContestQuestionUsers { get; set; }
    }
}

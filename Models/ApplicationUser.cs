using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuizWebApp.Models
{
    /*
     * Turns out that Entity Framework will assume that any class that inherits from a POCO class that is mapped to a table on the database 
     * requires a Discriminator column, even if the derived class will not be saved to the DB.
     * The solution is quite simple and you just need to add [NotMapped] as an attribute of the derived class.
     */
    //[NotMapped]
    public class ApplicationUser : IdentityUser
    {
        public bool isTemporary { get; set; }
        public virtual ICollection<ContestQuestionUser> ContestQuestionUsers { get; set; }
    }
}

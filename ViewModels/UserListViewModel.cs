using QuizWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.ViewModels
{
    public class UserListViewModel
    {
        public List<ApplicationUser> UserList { get; set; }
        public List<string> RoleNames { get; set; }
        public List<int> RolesCount { get; set; }
    }
}

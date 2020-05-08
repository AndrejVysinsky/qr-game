using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizWebApp.Services
{
    public class AuthMessageSenderOptions
    {
        public string MailGunDomain { get; set; }
        public string MailGunKey { get; set; }
    }
}

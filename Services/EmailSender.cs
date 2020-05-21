using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuizWebApp.Services
{
    /*
     * ZDROJ:
     * https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio
     * 
     */

    public class EmailSender : IEmailSender
    {
        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;

            Options = new AuthMessageSenderOptions
            {
                Address = Environment.GetEnvironmentVariable("SMARTHOST_ADDRESS") ?? Configuration["SMTPGmail:Address"],
                Port = Int32.Parse(Environment.GetEnvironmentVariable("SMARTHOST_PORT") ?? Configuration["SMTPGmail:Port"]),
                User = Environment.GetEnvironmentVariable("SMARTHOST_USER") ?? Configuration["SMTPGmail:User"],
                Password = Environment.GetEnvironmentVariable("SMARTHOST_PASSWORD") ?? Configuration["SMTPGmail:Password"]
            };
        }

        public IConfiguration Configuration { get; }
        public AuthMessageSenderOptions Options { get; set; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            SendMessage(Options.Address, Options.Port, Options.User, Options.Password, subject, message, email);
            return Task.CompletedTask;
        }

        public void SendMessage(string address, int port, string user, string password, string subject, string messageBody, string recipient)
        {
            using var message = new MailMessage();
            message.To.Add(new MailAddress(recipient, recipient));
            message.From = new MailAddress("info@frivia.uniza.sk", "FRIVIA");
            message.Subject = subject;
            message.Body = messageBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(address);
            client.Port = port;
            client.Credentials = new NetworkCredential(user, password);
            client.EnableSsl = true;
            client.Send(message);
        }
    }
}
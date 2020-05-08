using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
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
                MailGunDomain = Configuration["MailGun:Domain"],
                MailGunKey = Configuration["MailGun:Key"]
            };
        }

        public IConfiguration Configuration { get; }
        public AuthMessageSenderOptions Options { get; set; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            SendSimpleMessage(Options.MailGunDomain, Options.MailGunKey, subject, message, email);
            return Task.CompletedTask;
        }

        public IRestResponse SendSimpleMessage(string domain, string apiKey, string subject, string message, string email)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new System.Uri("https://api.mailgun.net/v3");
            client.Authenticator = new HttpBasicAuthenticator("api", apiKey);

            RestRequest request = new RestRequest();
            request.AddParameter("domain", domain, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "FRIVIA <frivia@sandboxafce95efa6824f6cb346d844c985bcda.mailgun.org>");
            request.AddParameter("to", email);
            request.AddParameter("subject", subject);
            request.AddParameter("text", message);
            request.Method = Method.POST;
            return client.Execute(request);
        }
    }
}
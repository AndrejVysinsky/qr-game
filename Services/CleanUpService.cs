using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuizWebApp.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QuizWebApp.Services
{
    public class CleanUpService : IHostedService
    {
        private Timer _timer;
        private readonly ILogger<CleanUpService> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public IServiceProvider Services { get; }

        public CleanUpService(IServiceProvider services, ILogger<CleanUpService> logger, IWebHostEnvironment hostEnvironment)
        {
            Services = services;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoCleanUpAsync, null, TimeSpan.Zero,
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void DoCleanUpAsync(object state)
        {
            var tempsPath = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/temps/");

            DirectoryInfo di = new DirectoryInfo(tempsPath);

            foreach (FileInfo file in di.GetFiles())
            {
                _logger.LogInformation($"Temp file {file.Name} was removed.");
                file.Delete();
            }

            using (var scope = Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

                var users = userManager.GetUsersInRoleAsync("User").Result;

                for (int i = users.Count - 1; i >= 0; i--)
                {
                    if (users[i].IsTemporary && ((DateTime.Now - users[i].RegistrationDate).Value.Days >= 14))
                    {
                        _logger.LogInformation($"User {users[i].Email} was removed because of expired account.");
                        userManager.DeleteAsync(users[i]);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}

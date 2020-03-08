using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizWebApp.Models;
using QuizWebApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Hangfire;
using System.Timers;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace QuizWebApp
{
    public class Startup
    {
        private Timer myTimer;
        private readonly IWebHostEnvironment _hostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            _hostEnvironment = hostEnvironment;

            myTimer = new Timer(24 * 60 * 60 * 1000); //one day in milliseconds
            myTimer.Elapsed += new ElapsedEventHandler(CleanUp);
            myTimer.Start();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>();


            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                //facebookOptions.CallbackPath = "/signin-facebook";
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            // requires
            // using Microsoft.AspNetCore.Identity.UI.Services;
            // using WebPWrecover.Services;
            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions { KnownNetworks = { new IPNetwork(IPAddress.Parse("172.21.0.0"), 16) }, ForwardedHeaders = ForwardedHeaders.All });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            CreateUserRoles(services).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            bool adminExists = await RoleManager.RoleExistsAsync("Admin");
            if (!adminExists)
            {
                await RoleManager.CreateAsync(new IdentityRole("Admin"));

                var user = new ApplicationUser { UserName = "admin@frivia.sk", Email = "admin@frivia.sk", RegistrationDate = DateTime.Now };
                await UserManager.CreateAsync(user, "admin123");
                await UserManager.AddToRoleAsync(user, "Admin");
            }

            bool modExists = await RoleManager.RoleExistsAsync("Moderator");
            if (!modExists)
                await RoleManager.CreateAsync(new IdentityRole("Moderator"));

            bool userExists = await RoleManager.RoleExistsAsync("User");
            if (!userExists)
                await RoleManager.CreateAsync(new IdentityRole("User")); 
         
        }

        private void CleanUp(object src, ElapsedEventArgs e)
        {
            var tempsPath = Path.Combine(_hostEnvironment.WebRootPath, @"uploads/temps/");

            DirectoryInfo di = new DirectoryInfo(tempsPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}


/*
 * dotnet user-secrets set SendGridUser MENO
 * dotnet user-secrets set SendGridKey apiKey
 */

//api key
//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



/*
* secrets.json
* 
* {
  "SendGridUser": "FRI QR Kvíz",
  "SendGridKey": "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",

  "Authentication:Facebook:AppId": "692703314556727",
  "Authentication:Facebook:AppSecret": "98b9f547aefac841f5377ccef31d5705"
}
* 
*/

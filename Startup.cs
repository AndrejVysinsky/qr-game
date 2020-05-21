using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuizWebApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizWebApp.Models;
using QuizWebApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace QuizWebApp
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var mySQLConnection = "server=localhost;port=3306;database=testik;user=root;password=password";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Environment.GetEnvironmentVariable("DATABASE_CONN_STR") 
                                        ?? mySQLConnection));

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
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            });

            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                //facebookOptions.CallbackPath = "/signin-facebook";
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddHostedService<CleanUpService>();            
        }

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

            UpdateDatabase(app);
            CreateUserRoles(services).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            bool adminRoleExists = await RoleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await RoleManager.CreateAsync(new IdentityRole("Admin"));

                var user = new ApplicationUser { UserName = "admin@frivia.sk", Email = "admin@frivia.sk", RegistrationDate = DateTime.Now };
                await UserManager.CreateAsync(user, "admin123");
                await UserManager.AddToRoleAsync(user, "Admin");
            }

            bool modRoleExists = await RoleManager.RoleExistsAsync("Moderator");
            if (!modRoleExists)
                await RoleManager.CreateAsync(new IdentityRole("Moderator"));

            bool userRoleExists = await RoleManager.RoleExistsAsync("User");
            if (!userRoleExists)
                await RoleManager.CreateAsync(new IdentityRole("User")); 
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                                                            .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
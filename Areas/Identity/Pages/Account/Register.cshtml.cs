using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuizWebApp.Data;
using QuizWebApp.Models;

namespace QuizWebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Zadajte email.")]
            [EmailAddress(ErrorMessage = "Zadaný email nie je platný.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Zadajte heslo.")]
            [StringLength(100, ErrorMessage = "{0} musí obsahovať od {2} do {1} znakov.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Heslo")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potvrdenie hesla")]
            [Compare("Password", ErrorMessage = "Zadané heslá sa nezhodujú.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, IsTemporary = false, RegistrationDate = DateTime.Now };
                var result = await _userManager.CreateAsync(user, Input.Password);

                await _userManager.AddToRoleAsync(user, "User");

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, code);

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public IActionResult OnPostSend([FromBody]string useremail)
        {
            bool exists = _context.ApplicationUsers.Any(user => user.Email == useremail);

            if (useremail.Contains("@frivia"))
                exists = true;

            return new JsonResult(exists);
        }
		
		public async Task<IActionResult> OnPostAnonymous(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/users/mycontests");

            int newID = GenerateGuestID();
            string guestEmail = "guest" + newID + "@frivia.sk";
            
            var user = new ApplicationUser { UserName = guestEmail, Email = guestEmail, IsTemporary = true, RegistrationDate = DateTime.Now };
            
            var result = await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, "User");

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                
                await _signInManager.SignInAsync(user, isPersistent: true);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return LocalRedirect(returnUrl);
        }

        public int GenerateGuestID()
        {
            var emails = _context.ApplicationUsers.Where(user => user.IsTemporary == true)
                                    .Select(user => user.Email)
                                    .Where(email => email.Contains("@frivia.sk")).ToList();

            int max = 0;
            foreach (var email in emails)
            {
                string extractNumber = Regex.Match(email, @"\d+").Value;
                int id = Int32.Parse(extractNumber);

                if (id > max)
                    max = id;
            }

            return max + 1;
        }
    }
}

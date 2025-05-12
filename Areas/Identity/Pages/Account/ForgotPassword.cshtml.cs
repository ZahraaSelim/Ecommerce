using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Ecommerce.Models;
using Ecommerce.Services;

namespace Ecommerce.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<User> userManager, ILogger<ForgotPasswordModel> logger, IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    TempData["Message"] = "If an account with that email exists, a password reset link will be sent.";
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Your Password",
                    $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>."
                );

                _logger.LogInformation("Password reset link generated for user {Email}: {CallbackUrl}", Input.Email, callbackUrl);
                TempData["Message"] = "If an account with that email exists, a password reset link has been sent.";
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
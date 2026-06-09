using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace shift_handover_page.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [Display(Name = "Username or email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Attempt to find user by email first, then by username
        var identifier = Input.Email?.Trim();
        IdentityUser? user = null;

        if (!string.IsNullOrEmpty(identifier))
        {
            user = await _userManager.FindByEmailAsync(identifier);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(identifier);
            }
        }

        if (user != null)
        {
            var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // If no user found, fall back to default sign-in attempt (keeps behavior consistent)
        if (!string.IsNullOrEmpty(identifier))
        {
            user = await _userManager.FindByEmailAsync(identifier);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(identifier);
            }

            if (user is not null)
            {
                var fallbackResult = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
                if (fallbackResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
            }
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}

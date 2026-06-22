using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CRM.Data;
using CRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CRM.Controllers
{
    [AllowAnonymous] // dostępny bez logowania
    public class AccountController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(CrmDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Możesz dostarczyć model z informacją co dokładnie odmówiono, lub sugestiami.
            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var user = await _context.GuiUsers.FirstOrDefaultAsync(u => u.Login == model.Login);
            if (user == null)
            {
                ModelState.AddModelError("", "Niepoprawny login lub hasło.");
                return View(model);
            }

            if (!VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Niepoprawny login lub hasło.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.NameIdentifier, user.Login)
                // dodaj role/other claims jeśli potrzebujesz
            };
            CRM.Models.Role? role = null;
            if (user.RoleId != 0)
            {
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            }

            if (role != null)
            {
                // role name jako standardowy claim typu Role
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                // dodatkowe "permission" claims (wartości "true" dla ułatwienia)
                if (role.CanManageEmployees) claims.Add(new Claim("CAN_MANAGE_EMPLOYEES", "true"));
                if (role.CanManageClients) claims.Add(new Claim("CAN_MANAGE_CLIENTS", "true"));
                if (role.CanManagePrices) claims.Add(new Claim("CAN_MANAGE_PRICES", "true"));
                if (role.CanManageInvoices) claims.Add(new Claim("CAN_MANAGE_INVOICES", "true"));
                if (role.CanMakeReservations) claims.Add(new Claim("CAN_MAKE_RESERVATIONS", "true"));
                if (role.CanManageGuiUsers) claims.Add(new Claim("CAN_MANAGE_GUI_USERS", "true"));
                if (role.CanManageServices) claims.Add(new Claim("CAN_MANAGE_SERVICES", "true"));
            }
            else
            {
                // brak roli — możesz dodać domyślny claim np. "Guest"
                // claims.Add(new Claim(ClaimTypes.Role, "Guest"));
            }

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);


            

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            _logger.LogInformation("User {Login} logged in.", user.Login);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Simple password verification that supports bcrypt and fallback sha256
        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // bcrypt hash starts with $2a$, $2b$, $2y$ etc.
            if (storedHash.StartsWith("$2"))
            {
                try
                {
                    // Requires BCrypt.Net-Next package
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }
                catch
                {
                    return false;
                }
            }

            // fallback: compare SHA256 hex
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha.ComputeHash(bytes);
            var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            return string.Equals(hex, storedHash.Trim().ToLowerInvariant(), StringComparison.Ordinal);
        }
    }

    public class LoginViewModel
    {
        [Required]
        public string Login { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
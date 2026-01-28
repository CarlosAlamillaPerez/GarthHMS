using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarthHMS.Core.DTOs;
using GarthHMS.Core.Interfaces.Services;

namespace GarthHMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthService authService,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var (success, user, errorMessage) = await _authService.LoginAsync(model);

                if (!success || user == null)
                {
                    ModelState.AddModelError(string.Empty, errorMessage ?? "Error al iniciar sesión");
                    return View(model);
                }

                // Crear claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("Role", user.UserRole.ToString()),
                    new Claim("MaxDiscount", user.MaxDiscountPercent.ToString())
                };

                // Agregar HotelId si no es SuperAdmin
                if (user.HotelId.HasValue)
                {
                    claims.Add(new Claim("HotelId", user.HotelId.Value.ToString()));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                // Redirigir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión. Intente nuevamente");
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _authService.LogoutAsync();

                _logger.LogInformation("User logged out");

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login", "Account");
            }
        }

        // GET: /Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
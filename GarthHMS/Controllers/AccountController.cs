using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarthHMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IHotelSettingsService _hotelSettingsService;

        public AccountController(
            IAuthService authService,
            IHotelSettingsService hotelSettingsService)
        {
            _authService = authService;
            _hotelSettingsService = hotelSettingsService;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
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

            var (success, user, errorMessage) = await _authService.LoginAsync(model);

            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error al iniciar sesión");
                return View(model);
            }

            // Crear claims del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.UserRoleText),
                new Claim("MaxDiscount", user.MaxDiscountPercent.ToString())
            };

            // Si no es SuperAdmin, agregar HotelId y OperationMode
            if (user.HotelId != Guid.Empty)
            {
                claims.Add(new Claim("HotelId", user.HotelId.ToString()));

                // Obtener OperationMode desde HotelSettings
                var settingsResult = await _hotelSettingsService.GetSettingsAsync(user.HotelId);
                var operationMode = settingsResult.IsSuccess
                    ? settingsResult.Data?.OperationMode ?? "hotel"
                    : "hotel";
                claims.Add(new Claim("OperationMode", operationMode));
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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _authService.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarthHMS.Infrastructure.Data;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MultiTenancyHelper _multiTenancyHelper;

        public HomeController(
            ILogger<HomeController> logger,
            MultiTenancyHelper multiTenancyHelper)
        {
            _logger = logger;
            _multiTenancyHelper = multiTenancyHelper;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            // Obtener información del usuario actual
            var userId = _multiTenancyHelper.GetCurrentUserId();
            var hotelId = _multiTenancyHelper.GetCurrentHotelId();
            var role = _multiTenancyHelper.GetCurrentUserRole();
            var isSuperAdmin = _multiTenancyHelper.IsSuperAdmin();

            ViewBag.UserId = userId;
            ViewBag.HotelId = hotelId;
            ViewBag.Role = role;
            ViewBag.IsSuperAdmin = isSuperAdmin;

            return View();
        }

        // GET: /Home/Error
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
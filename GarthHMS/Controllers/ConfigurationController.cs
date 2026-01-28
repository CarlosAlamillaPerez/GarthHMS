using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarthHMS.Web.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(ILogger<ConfigurationController> logger)
        {
            _logger = logger;
        }

        // GET: /Configuration/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
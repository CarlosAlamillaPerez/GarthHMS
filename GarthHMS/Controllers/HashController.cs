using Microsoft.AspNetCore.Mvc;

namespace GarthHMS.Web.Controllers
{
    public class HashController : Controller
    {
        [HttpGet]
        public IActionResult Generate()
        {
            var password1 = "ZyanRooms2025!";
            var hash1 = BCrypt.Net.BCrypt.HashPassword(password1, BCrypt.Net.BCrypt.GenerateSalt(12));

            var html = $@"
                <h1>Password Hashes</h1>
                <hr>
                <h3>Password: {password1}</h3>
                <p><strong>Hash:</strong> {hash1}</p>
                <hr>
            ";

            return Content(html, "text/html");
        }
    }
}
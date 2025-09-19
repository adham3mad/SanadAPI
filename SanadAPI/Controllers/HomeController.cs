using Microsoft.AspNetCore.Mvc;

namespace SanadAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "🚀 Sanad API is running successfully",
                swagger = "/swagger"
            });
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace zen_demo_dotnet.Controllers
{
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("test_ratelimiting_1")]
        public IActionResult TestRateLimiting1()
        {
            return Content("Request successful (Ratelimiting 1)");
        }

        [HttpGet("test_ratelimiting_2")]
        public IActionResult TestRateLimiting2()
        {
            return Content("Request successful (Ratelimiting 2)");
        }

        [HttpGet("test_bot_blocking")]
        public IActionResult TestBotBlocking()
        {
            return Content("Hello World! Bot blocking enabled on this route.");
        }

        [HttpGet("test_user_blocking")]
        public IActionResult TestUserBlocking()
        {
            Request.Headers.TryGetValue("user", out var user);
            return Content($"Hello User with id: {user}");
        }

        [HttpGet("test_endpoint_ip_blocking/{id:int}")]
        public IActionResult TestEndpointIpBlocking(int id)
        {
            Request.Headers.TryGetValue("user", out var user);
            return Content($"Hello endpoint with route parameter: {id}");
        }

    }
}

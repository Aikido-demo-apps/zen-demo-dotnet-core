using Microsoft.AspNetCore.Mvc;

namespace zen_demo_dotnet.Controllers
{
    public class PagesController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }

        [HttpGet("pages/execute")]
        public IActionResult Execute()
        {
            return File("~/execute_command.html", "text/html");
        }

        [HttpGet("pages/create")]
        public IActionResult Create()
        {
            return File("~/create.html", "text/html");
        }

        [HttpGet("pages/request")]
        public IActionResult HttpRequest()
        {
            return File("~/request.html", "text/html");
        }

        [HttpGet("pages/read")]
        public IActionResult Read()
        {
            return File("~/read_file.html", "text/html");
        }
    }
}

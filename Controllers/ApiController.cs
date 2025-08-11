using Microsoft.AspNetCore.Mvc;
using zen_demo_dotnet.Data;
using zen_demo_dotnet.Helpers;
using zen_demo_dotnet.Models;
using System.Text.RegularExpressions;

namespace zen_demo_dotnet.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly DatabaseHelper _databaseHelper;
        private readonly AppHelpers _appHelpers;
        private readonly ILogger<ApiController> _logger;

        public ApiController(
            DatabaseHelper databaseHelper,
            AppHelpers appHelpers,
            ILogger<ApiController> logger)
        {
            _databaseHelper = databaseHelper;
            _appHelpers = appHelpers;
            _logger = logger;
        }

        [HttpGet("api/pets")]
        public async Task<IActionResult> GetPets()
        {
            var pets = await _databaseHelper.GetAllPetsAsync();
            _logger.LogInformation("Retrieved {Count} pets", pets.Count);
            return Ok(pets);
        }

        [HttpPost("api/create")]
        public async Task<IActionResult> CreatePet([FromBody] CreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Name is required");
            }

            var rowsCreated = await _databaseHelper.CreatePetByNameAsync(request.Name);
            if (rowsCreated == -1)
            {
                return StatusCode(500, "Database error occurred");
            }
            
            return Ok("Success!");
        }

        [HttpPost("api/execute")]
        public IActionResult ExecuteCommandPost([FromBody] CommandRequest request)
        {
            if (string.IsNullOrEmpty(request.UserCommand))
            {
                return BadRequest("Command is required");
            }

            var result = _appHelpers.ExecuteShellCommand(request.UserCommand);
            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpGet("api/execute/{command}")]
        public IActionResult ExecuteCommandGet(string command)
        {
            var result = _appHelpers.ExecuteShellCommand(command);
            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpPost("api/request")]
        public async Task<IActionResult> MakeRequest([FromBody] RequestRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required");
            }

            var response = await _appHelpers.MakeHttpRequestAsync(request.Url);
            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpPost("api/request2")]
        public async Task<IActionResult> MakeRequest2([FromBody] RequestRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required");
            }

            var response = await _appHelpers.MakeHttpRequestAsync(request.Url);
            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpPost("api/request_different_port")]
        public async Task<IActionResult> MakeRequestDifferentPort([FromBody] RequestRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required");
            }
            if (string.IsNullOrEmpty(request.Port))
            {
                return BadRequest("Port is required");
            }
            var newUrl = Regex.Replace(request.Url, @":\d+", ":" + request.Port);
            
            var response = await _appHelpers.MakeHttpRequestAsync(newUrl);
            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpGet("api/read")]
        public IActionResult ReadFile([FromQuery] string? path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return StatusCode(500, "Path is required");
            }

            var response = _appHelpers.ReadFile(path);
            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpGet("api/read2")]
        public IActionResult ReadFile2([FromQuery] string? path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return StatusCode(500, "Path is required");
            }

            var response = _appHelpers.ReadFile2(path);
            return StatusCode(response.StatusCode, response.Message);
        }

        [HttpGet("api/client-ip")]
        public IActionResult GetClientIp()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var aikidoIp = ((dynamic)HttpContext.Items["Aikido.Zen.Context"]).RemoteAddress;
            return Ok($"{ip}\r\n{aikidoIp}\r\n{HttpContext.Request.Headers["X-Forwarded-For"]}");
        }

        [HttpGet("clear")]
        public async Task<IActionResult> Clear()
        {
            await _databaseHelper.ClearAllAsync();
            return Ok("Cleared successfully.");
        }
    }
}

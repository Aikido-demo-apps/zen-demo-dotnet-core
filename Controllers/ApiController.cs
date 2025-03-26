using Microsoft.AspNetCore.Mvc;
using zen_demo_dotnet.Data;
using zen_demo_dotnet.Helpers;
using zen_demo_dotnet.Models;

namespace zen_demo_dotnet.Controllers
{
    [ApiController]
    [Route("api")]
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

        [HttpGet("pets")]
        public async Task<IActionResult> GetPets()
        {
            var pets = await _databaseHelper.GetAllPetsAsync();
            _logger.LogInformation("Retrieved {Count} pets", pets.Count);
            return Ok(pets);
        }

        [HttpPost("create")]
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

        [HttpPost("execute")]
        public IActionResult ExecuteCommandPost([FromBody] CommandRequest request)
        {
            if (string.IsNullOrEmpty(request.UserCommand))
            {
                return BadRequest("Command is required");
            }

            var result = _appHelpers.ExecuteShellCommand(request.UserCommand);
            return Ok(result);
        }

        [HttpGet("execute/{command}")]
        public IActionResult ExecuteCommandGet(string command)
        {
            var result = _appHelpers.ExecuteShellCommand(command);
            return Ok(result);
        }

        [HttpPost("request")]
        public async Task<IActionResult> MakeRequest([FromBody] RequestRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required");
            }

            var response = await _appHelpers.MakeHttpRequestAsync(request.Url);
            return Ok(response);
        }

        [HttpPost("request2")]
        public async Task<IActionResult> MakeRequest2([FromBody] RequestRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("URL is required");
            }

            var response = await _appHelpers.MakeHttpRequestAsync(request.Url);
            return Ok(response);
        }

        [HttpGet("read")]
        public IActionResult ReadFile([FromQuery] string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return BadRequest("Path is required");
            }

            var content = _appHelpers.ReadFile(path);
            return Ok(content);
        }

        [HttpGet("clear")]
        public async Task<IActionResult> Clear()
        {
            await _databaseHelper.ClearAllAsync();
            return Ok("Cleared successfully.");
        }
    }
}

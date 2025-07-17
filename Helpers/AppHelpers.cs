using System.Diagnostics;
using zen_demo_dotnet.Models;
using Aikido.Zen.Core.Exceptions;


namespace zen_demo_dotnet.Helpers
{
    public class AppHelpers
    {
        private readonly ILogger<AppHelpers> _logger;
        private readonly HttpClient _httpClient;

        public AppHelpers(ILogger<AppHelpers> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public AppResponse ExecuteShellCommand(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Environment.OSVersion.Platform == PlatformID.Unix ? "/bin/bash" : "cmd.exe",
                    Arguments = $"-c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();
                
                return new AppResponse
                {
                    StatusCode = 200,
                    Message = output
                };
                
            }
            catch (AikidoException ex)
            {
                _logger.LogError(ex, "Error executing shell command");
                return new AppResponse
                {
                    StatusCode = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing shell command");
                return new AppResponse
                {
                    StatusCode = 400,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<AppResponse> MakeHttpRequestAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return new AppResponse
                {
                    StatusCode = 200,
                    Message = content
                };
            }
            catch (AikidoException ex)
            {
                _logger.LogError(ex, "Error making HTTP request");
                return new AppResponse
                {
                    StatusCode = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making HTTP request");
                return new AppResponse
                {
                    StatusCode = 400,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public AppResponse ReadFile(string filePath)
        {
            try
            {
                // Construct the full path within the wwwroot/blogs directory
                string fullPath = Path.Combine("wwwroot/blogs", filePath);
                
                if (File.Exists(fullPath))
                {
                    var content = File.ReadAllText(fullPath);
                    return new AppResponse
                    {
                        StatusCode = 200,
                        Message = content
                    };
                }
                return new AppResponse
                {
                    StatusCode = 404,
                    Message = "File not found"
                };
            }
            catch (AikidoException ex)
            {
                _logger.LogError(ex, "Error reading file");
                return new AppResponse
                {
                    StatusCode = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                return new AppResponse
                {
                    StatusCode = 400,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}

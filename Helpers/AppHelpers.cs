using System.Diagnostics;
using zen_demo_dotnet.Models;

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

        public CommandResponse ExecuteShellCommand(string command)
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
                
                if (string.IsNullOrEmpty(error))
                {
                    return new CommandResponse
                    {
                        StatusCode = 200,
                        Message = output
                    };
                }
                else
                {
                    return new CommandResponse
                    {
                        StatusCode = 500,
                        Message = error
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing shell command");
                return new CommandResponse
                {
                    StatusCode = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<string> MakeHttpRequestAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making HTTP request");
                return $"Error: {ex.Message}";
            }
        }

        public string ReadFile(string filePath)
        {
            try
            {
                // Construct the full path within the wwwroot/blogs directory
                string fullPath = Path.Combine("wwwroot/blogs", filePath);
                
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
                return "File not found";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file");
                return $"Error: {ex.Message}";
            }
        }
    }
}

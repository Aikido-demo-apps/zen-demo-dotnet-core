using System.Diagnostics;

namespace zen_demo_dotnet.Helpers
{
    public class AppHelpers
    {
        private const int MaxDecodeUriPasses = 2;

        private readonly HttpClient _httpClient;

        public AppHelpers()
        {
            _httpClient = new HttpClient();
        }

        public string ExecuteShellCommand(string command)
        {
            command = DecodeUriComponent(command);

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
                
            return string.IsNullOrEmpty(error) ? output : error;
        }

        public async Task<string> MakeHttpRequestAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public string ReadFile(string filePath)
        {
            // Construct the full path within the wwwroot/blogs directory
            string fullPath = Path.Combine("wwwroot/blogs", filePath);

            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            return "File not found";
        }

        public string ReadFile2(string filePath)
        {
            // Use Path.GetFullPath to resolve the path (similar to path.resolve in Node.js)
            string fullPath = Path.GetFullPath(Path.Combine("wwwroot/blogs", filePath));

            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            return "File not found";
        }

        public async Task<string> MakeHttpRequestDifferentPortAsync(string url, int port)
        {
            // Replace the port in the URL
            var uri = new Uri(url);
            var uriBuilder = new UriBuilder(uri)
            {
                Port = port
            };

            var response = await _httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static string DecodeUriComponent(string input)
        {
            string decoded = input;

            if (string.IsNullOrEmpty(input))
            {
                return decoded;
            }

            for (int i = 0; i < MaxDecodeUriPasses; i++)
            {
                string next = Uri.UnescapeDataString(decoded);
                if (next == decoded)
                {
                    break;
                }

                decoded = next;
            }

            return decoded;
        }
    }
}

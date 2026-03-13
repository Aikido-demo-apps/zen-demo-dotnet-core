using System.Collections.Concurrent;
using System.Diagnostics;

namespace zen_demo_dotnet.Helpers
{
    public class AppHelpers
    {
        private static readonly string[] StoredSsrfUrls =
        {
            "http://evil-stored-ssrf-hostname/latest/api/token",
            "http://metadata.google.internal/latest/api/token",
            "http://metadata.goog/latest/api/token",
            "http://169.254.169.254/latest/api/token",
        };

        private readonly ILogger<AppHelpers> _logger;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, byte> _storedSsrfUrls = new(StringComparer.OrdinalIgnoreCase);

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
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (AikidoException)
            {
                // Let Aikido exceptions bubble up to be handled by middleware
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making HTTP request");
                return $"Error: {ex.Message}";
            }
        }

        public Task<string> MakeStoredSsrfRequestAsync(int? urlIndex)
        {
            int resolvedIndex = (urlIndex ?? 0) % StoredSsrfUrls.Length;
            return MakeHttpRequestAsync(StoredSsrfUrls[resolvedIndex]);
        }

        public void QueueStoredSsrfRequest()
        {
            _storedSsrfUrls.TryAdd(StoredSsrfUrls[0], 0);
        }

        public async Task ProcessStoredSsrfQueueAsync(CancellationToken cancellationToken)
        {
            if (_storedSsrfUrls.IsEmpty)
            {
                return;
            }

            _logger.LogInformation("Running stored SSRF check at {Timestamp}", DateTimeOffset.UtcNow);

            foreach (string url in _storedSsrfUrls.Keys)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, cancellationToken);
                    await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation("Fetched {Url} (status: {StatusCode})", url, (int)response.StatusCode);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching stored SSRF URL {Url}", url);
                }
                finally
                {
                    _storedSsrfUrls.TryRemove(url, out _);
                }
            }
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

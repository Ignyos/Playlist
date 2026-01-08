using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace Playlist.Services
{
    public class UpdateService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/Ignyos/Playlist/releases/latest";
        private readonly HttpClient _httpClient;

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Playlist-App");
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                var latestVersion = await GetLatestVersionFromGitHubAsync();

                if (latestVersion == null)
                {
                    return new UpdateCheckResult
                    {
                        IsUpdateAvailable = false,
                        ErrorMessage = "Unable to check for updates. Please try again later."
                    };
                }

                var isNewer = IsNewerVersion(currentVersion, latestVersion.Version);

                return new UpdateCheckResult
                {
                    IsUpdateAvailable = isNewer,
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion.Version,
                    DownloadUrl = latestVersion.HtmlUrl ?? string.Empty,
                    ReleaseNotes = latestVersion.Body ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult
                {
                    IsUpdateAvailable = false,
                    ErrorMessage = $"Error checking for updates: {ex.Message}"
                };
            }
        }

        public string GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version?.Major}.{version?.Minor}.{version?.Build}";
        }

        private async Task<GitHubRelease?> GetLatestVersionFromGitHubAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(GitHubApiUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return release;
            }
            catch
            {
                return null;
            }
        }

        private bool IsNewerVersion(string currentVersion, string latestVersion)
        {
            try
            {
                // Remove 'v' prefix if present
                currentVersion = currentVersion.TrimStart('v');
                latestVersion = latestVersion.TrimStart('v');

                var current = Version.Parse(currentVersion);
                var latest = Version.Parse(latestVersion);

                return latest > current;
            }
            catch
            {
                return false;
            }
        }
    }

    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class GitHubRelease
    {
        public string? TagName { get; set; }
        public string? Name { get; set; }
        public string? Body { get; set; }
        public string? HtmlUrl { get; set; }
        
        public string Version => TagName?.TrimStart('v') ?? "0.0.0";
    }
}

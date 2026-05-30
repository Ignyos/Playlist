using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;

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

                if (!TryParseVersion(currentVersion, out var currentParsed))
                {
                    return new UpdateCheckResult
                    {
                        IsUpdateAvailable = false,
                        ErrorMessage = "Unable to determine the current application version."
                    };
                }

                if (!TryParseVersion(latestVersion.Version, out var latestParsed))
                {
                    return new UpdateCheckResult
                    {
                        IsUpdateAvailable = false,
                        ErrorMessage = "Unable to parse the latest release version from GitHub."
                    };
                }

                var isNewer = latestParsed > currentParsed;

                return new UpdateCheckResult
                {
                    IsUpdateAvailable = isNewer,
                    CurrentVersion = FormatVersion(currentParsed),
                    LatestVersion = FormatVersion(latestParsed),
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
            var version = Assembly.GetEntryAssembly()?.GetName().Version
                ?? Assembly.GetExecutingAssembly().GetName().Version;

            if (version == null)
            {
                return "0.0.0";
            }

            return FormatVersion(version);
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

        private static bool TryParseVersion(string input, out Version version)
        {
            version = new Version(0, 0, 0);

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var match = Regex.Match(input, @"\d+(?:\.\d+){1,3}");
            if (!match.Success)
            {
                return false;
            }

            if (!Version.TryParse(match.Value, out var parsed) || parsed == null)
            {
                return false;
            }

            version = parsed;
            return true;
        }

        private static string FormatVersion(Version version)
        {
            var build = version.Build >= 0 ? version.Build : 0;
            return $"{version.Major}.{version.Minor}.{build}";
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
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
        
        public string Version => TagName?.TrimStart('v') ?? "0.0.0";
    }
}

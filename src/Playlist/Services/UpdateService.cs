using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Playlist.Services
{
    public class UpdateService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/Ignyos/Playlist/releases?per_page=50";
        private const string InstallerAssetName = "PlaylistSetup.exe";
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

                var installerAsset = latestVersion.Assets?
                    .FirstOrDefault(asset => string.Equals(asset.Name, InstallerAssetName, StringComparison.OrdinalIgnoreCase));

                if (installerAsset == null || string.IsNullOrWhiteSpace(installerAsset.BrowserDownloadUrl))
                {
                    return new UpdateCheckResult
                    {
                        IsUpdateAvailable = false,
                        ErrorMessage = "Unable to find the installer for the latest release."
                    };
                }

                var isNewer = latestParsed > currentParsed;

                return new UpdateCheckResult
                {
                    IsUpdateAvailable = isNewer,
                    CurrentVersion = FormatVersion(currentParsed),
                    LatestVersion = FormatVersion(latestParsed),
                    DownloadUrl = installerAsset.BrowserDownloadUrl,
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
                var releases = JsonSerializer.Deserialize<List<GitHubRelease>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (releases == null || releases.Count == 0)
                {
                    return null;
                }

                var stableCandidates = releases
                    .Where(r => !r.Draft && !r.Prerelease)
                    .Select(r => new
                    {
                        Release = r,
                        HasInstaller = r.Assets?.Any(a =>
                            string.Equals(a.Name, InstallerAssetName, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(a.BrowserDownloadUrl)) == true,
                        Parsed = TryParseVersion(r.Version, out var parsed) ? parsed : null
                    })
                    .Where(x => x.HasInstaller && x.Parsed != null)
                    .OrderByDescending(x => x.Parsed)
                    .ThenByDescending(x => x.Release.PublishedAt)
                    .ToList();

                if (stableCandidates.Count > 0)
                {
                    return stableCandidates[0].Release;
                }

                var prereleaseCandidates = releases
                    .Where(r => !r.Draft && r.Prerelease)
                    .Select(r => new
                    {
                        Release = r,
                        HasInstaller = r.Assets?.Any(a =>
                            string.Equals(a.Name, InstallerAssetName, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(a.BrowserDownloadUrl)) == true,
                        Parsed = TryParseVersion(r.Version, out var parsed) ? parsed : null
                    })
                    .Where(x => x.HasInstaller && x.Parsed != null)
                    .OrderByDescending(x => x.Parsed)
                    .ThenByDescending(x => x.Release.PublishedAt)
                    .ToList();

                return prereleaseCandidates.Count > 0 ? prereleaseCandidates[0].Release : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> DownloadInstallerAsync(string downloadUrl)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentException("A download URL is required.", nameof(downloadUrl));
            }

            var uri = new Uri(downloadUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = InstallerAssetName;
            }

            var downloadDirectory = Path.Combine(Path.GetTempPath(), "PlaylistUpdates");
            Directory.CreateDirectory(downloadDirectory);

            var downloadPath = Path.Combine(downloadDirectory, fileName);

            try
            {
                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using var remoteStream = await response.Content.ReadAsStreamAsync();
                await using var localStream = File.Create(downloadPath);
                await remoteStream.CopyToAsync(localStream);

                return downloadPath;
            }
            catch
            {
                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }

                throw;
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

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset>? Assets { get; set; }
        
        public string Version => TagName?.TrimStart('v') ?? "0.0.0";
    }

    public class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}

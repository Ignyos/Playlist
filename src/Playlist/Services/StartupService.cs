using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Playlist.Services;

public static class StartupService
{
    private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string AppName = "Playlist";

    public static void ApplyRunOnStartup(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open registry key for startup.");

        var exePath = Process.GetCurrentProcess().MainModule?.FileName
            ?? throw new InvalidOperationException("Unable to determine application path.");

        if (enable)
        {
            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }

    public static bool IsRunOnStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(AppName) as string;
        return !string.IsNullOrWhiteSpace(value);
    }
}

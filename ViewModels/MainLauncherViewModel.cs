using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinHunterLauncher.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection;
using SkinHunterLauncher.Services;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic; // Para List<SupabaseUpdateLogEntry>

namespace SkinHunterLauncher.ViewModels
{
    public partial class MainLauncherViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;
        private readonly CurrentUserSessionService _sessionService;
        private readonly SupabaseService _supabaseService;
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private string? _userLogin;

        [ObservableProperty]
        private string? _userAvatarFallback;

        [ObservableProperty]
        private string _patchVersion = "Unknown";

        [ObservableProperty]
        private string _versionStatus = "Checking...";

        [ObservableProperty]
        private string _licenseType = "N/A";

        public ObservableCollection<SupabaseUpdateLogEntry> UpdateLogs { get; } = new();

        public MainLauncherViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            _sessionService = serviceProvider.GetRequiredService<CurrentUserSessionService>();
            _supabaseService = serviceProvider.GetRequiredService<SupabaseService>();
            _httpClient = new HttpClient();

            Title = "Skin-Hunter - Home";
        }

        private void LoadUserDataAndLicense()
        {
            if (_sessionService.IsUserLoggedIn && _sessionService.CurrentUser != null)
            {
                UserLogin = _sessionService.CurrentUser.Login;
                if (!string.IsNullOrEmpty(UserLogin))
                {
                    UserAvatarFallback = UserLogin.Length > 0 ? UserLogin[0].ToString().ToUpper() : "U";
                }
                else
                {
                    UserLogin = "User";
                    UserAvatarFallback = "U";
                }
                LicenseType = _sessionService.CurrentUser.IsBuyer ? "Buyer" : "N/A";
            }
            else
            {
                UserLogin = "Guest";
                UserAvatarFallback = "G";
                LicenseType = "N/A";
            }
        }

        private async Task FetchAndUpdateLogs()
        {
            var logs = await _supabaseService.GetUpdateLogsAsync();
            UpdateLogs.Clear();
            if (logs != null)
            {
                // Ordenar por alguna propiedad si es necesario, por ejemplo, una fecha o versión si la añades.
                // Por ahora, los añade tal como vienen.
                foreach (var logEntry in logs)
                {
                    UpdateLogs.Add(logEntry);
                }
            }
            else
            {
                // Añadir un log por defecto si falla la carga
                UpdateLogs.Add(new SupabaseUpdateLogEntry { Title = "INFO", Changes = new List<string> { "Could not load update logs." } });
            }
        }

        private async Task CheckVersion()
        {
            VersionStatus = "Checking...";
            try
            {
                string cdragonVersionString = "";
                try
                {
                    var responseCdragon = await _httpClient.GetAsync("https://raw.communitydragon.org/latest/content-metadata.json");
                    responseCdragon.EnsureSuccessStatusCode();
                    string jsonCdragon = await responseCdragon.Content.ReadAsStringAsync();
                    using var docCdragon = JsonDocument.Parse(jsonCdragon);
                    if (docCdragon.RootElement.TryGetProperty("version", out JsonElement versionElement))
                    {
                        var fullCdragonVersion = versionElement.GetString();
                        var versionParts = fullCdragonVersion?.Split('.').Take(2);
                        if (versionParts != null)
                        {
                            cdragonVersionString = string.Join(".", versionParts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching CDRAGON version: {ex.Message}");
                    PatchVersion = "DB Error";
                    VersionStatus = "Error";
                    return;
                }

                if (string.IsNullOrEmpty(cdragonVersionString))
                {
                    PatchVersion = "N/A";
                    VersionStatus = "Unknown";
                    return;
                }

                PatchVersion = cdragonVersionString;

                string supabasePatchVersionString = "";
                try
                {
                    byte[]? fileBytes = await _supabaseService.DownloadFileBytesAsync("version", "patch.json");

                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        Debug.WriteLine($"Supabase patch.json not found or is empty.");
                        VersionStatus = "Local N/A";
                        return;
                    }
                    string jsonSupabase = System.Text.Encoding.UTF8.GetString(fileBytes);

                    using var docSupabase = JsonDocument.Parse(jsonSupabase);
                    if (docSupabase.RootElement.TryGetProperty("version", out JsonElement supabaseVersionElement))
                    {
                        var fullSupabaseVersion = supabaseVersionElement.GetString();
                        var versionParts = fullSupabaseVersion?.Split('.').Take(2);
                        if (versionParts != null)
                        {
                            supabasePatchVersionString = string.Join(".", versionParts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching/parsing Supabase patch.json: {ex.Message}");
                    VersionStatus = "Local Error";
                    return;
                }

                if (string.IsNullOrEmpty(supabasePatchVersionString))
                {
                    VersionStatus = "Local N/A";
                    return;
                }

                if (cdragonVersionString.Equals(supabasePatchVersionString, StringComparison.OrdinalIgnoreCase))
                {
                    VersionStatus = "UPDATED";
                }
                else
                {
                    VersionStatus = "OUTDATED";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking version: {ex.Message}");
                PatchVersion = "Error";
                VersionStatus = "Error";
            }
        }

        [RelayCommand]
        private void Play()
        {
            string mainAppExecutableName = "SkinHunterWPF.exe";
            string launcherDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mainAppPath = Path.Combine(launcherDirectory, mainAppExecutableName);

            if (!File.Exists(mainAppPath))
            {
                Debug.WriteLine($"Primary path not found: {mainAppPath}. Attempting development path discovery...");
                try
                {
                    DirectoryInfo? currentDirInfo = new DirectoryInfo(launcherDirectory);
                    DirectoryInfo? binDir = currentDirInfo.Parent?.Parent;
                    DirectoryInfo? projectLauncherDir = binDir?.Parent;
                    DirectoryInfo? solutionDir = projectLauncherDir?.Parent;

                    if (solutionDir != null)
                    {
                        string skinHunterWPFProjectDirName = "SkinHunterWPF";
                        string targetFramework = currentDirInfo.Name;
                        string configuration = currentDirInfo.Parent?.Name ?? "Debug";

                        string devMainAppPath = Path.Combine(solutionDir.FullName, skinHunterWPFProjectDirName, "bin", configuration, targetFramework, mainAppExecutableName);
                        Debug.WriteLine($"Attempting development path: {devMainAppPath}");
                        if (File.Exists(devMainAppPath))
                        {
                            mainAppPath = devMainAppPath;
                        }
                        else
                        {
                            Debug.WriteLine($"Development path also not found: {devMainAppPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during development path discovery: {ex.Message}");
                }
            }

            if (File.Exists(mainAppPath))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(mainAppPath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);

                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start SkinHunterWPF:\n{ex.Message}", "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"SkinHunterWPF.exe not found.\nSearched at: {mainAppPath}\n(And common development paths)\n\nPlease ensure SkinHunterWPF.exe is in the same directory as the launcher or that build output paths are correctly configured for development.",
                                "Application Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override async Task InitializeAsync(object? parameter = null)
        {
            LoadUserDataAndLicense();
            await FetchAndUpdateLogs();
            await CheckVersion();
        }
    }
}
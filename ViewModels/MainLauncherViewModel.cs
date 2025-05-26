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

namespace SkinHunterLauncher.ViewModels
{
    public partial class MainLauncherViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;
        private readonly CurrentUserSessionService _sessionService; // CAMBIADO
        private readonly SupabaseService _supabaseService;
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private string? _userLogin;

        [ObservableProperty]
        private string? _userAvatarFallback;

        [ObservableProperty]
        private string _patchVersion = "Patch: Unknown";

        [ObservableProperty]
        private string _versionStatus = "Checking...";

        [ObservableProperty]
        private string _licenseType = "License: Free";


        public ObservableCollection<UpdateLogEntry> UpdateLogs { get; } = [];

        public MainLauncherViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            _sessionService = serviceProvider.GetRequiredService<CurrentUserSessionService>(); // CAMBIADO
            _supabaseService = serviceProvider.GetRequiredService<SupabaseService>();
            _httpClient = new HttpClient();

            Title = "Skin-Hunter - Home";
            LoadUpdateLogs();
            LoadUserDataAndLicense(); // CAMBIADO
        }

        private void LoadUserDataAndLicense()
        {
            if (_sessionService.IsUserLoggedIn && _sessionService.CurrentUser != null)
            {
                UserLogin = _sessionService.CurrentUser.Login;
                if (!string.IsNullOrEmpty(UserLogin))
                {
                    UserAvatarFallback = UserLogin.FirstOrDefault().ToString().ToUpper();
                }
                else // Por si el login es nulo/vacío por alguna razón
                {
                    UserLogin = "User";
                    UserAvatarFallback = "U";
                }
                LicenseType = _sessionService.CurrentUser.IsBuyer ? "License: Buyer" : "License: Free";
            }
            else
            {
                UserLogin = "Guest";
                UserAvatarFallback = "G";
                LicenseType = "License: N/A";
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
                    PatchVersion = "Patch: Cdragon Error";
                    VersionStatus = "Error";
                    return;
                }

                if (string.IsNullOrEmpty(cdragonVersionString))
                {
                    PatchVersion = "Patch: N/A";
                    VersionStatus = "Unknown";
                    return;
                }

                PatchVersion = $"Patch: {cdragonVersionString}";

                string supabasePatchVersionString = "";
                try
                {
                    string bucketName = "version";
                    string filePathInBucket = "patch.json";
                    byte[]? fileBytes = await _supabaseService.DownloadFileBytesAsync(bucketName, filePathInBucket);

                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        Debug.WriteLine($"Supabase patch.json not found or is empty in bucket '{bucketName}'.");
                        VersionStatus = "Local N/A";
                        return;
                    }
                    string jsonSupabase = System.Text.Encoding.UTF8.GetString(fileBytes);

                    using var docSupabase = JsonDocument.Parse(jsonSupabase);
                    // Para tu JSON: {"version": "15.10.6804378+branch.releases-15-10.content.release"}
                    // necesitas extraer la parte X.Y
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
                PatchVersion = "Patch: Error";
                VersionStatus = "Error";
            }
        }

        private void LoadUpdateLogs()
        {
            UpdateLogs.Add(new UpdateLogEntry
            {
                Title = "UPDATE",
                Changes = ["Fixed crashes", "Update log", "Update log 1"]
            });
            UpdateLogs.Add(new UpdateLogEntry
            {
                Title = "UPDATE",
                Changes = ["New UI", "Update log", "Update log 1"]
            });
            UpdateLogs.Add(new UpdateLogEntry
            {
                Title = "UPDATE",
                Changes = ["Performance improvements", "Added new feature X", "Bug fixes for Y"]
            });
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
            LoadUserDataAndLicense(); // Asegurarse de cargar datos de usuario aquí también por si hay navegación directa
            await CheckVersion();
        }
    }
}
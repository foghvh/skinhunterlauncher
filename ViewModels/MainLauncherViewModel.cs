using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkinHunterLauncher.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection; // Necesario para GetService

namespace SkinHunterLauncher.ViewModels
{
    public partial class MainLauncherViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;

        [ObservableProperty]
        private string _gameName = "CS2";

        [ObservableProperty]
        private string _status = "safe";

        [ObservableProperty]
        private string _expiryDate = "05.10";

        public ObservableCollection<UpdateLogEntry> UpdateLogs { get; } = [];

        public MainLauncherViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            Title = "Skin-Hunter - Home"; // REBRANDED
            LoadUpdateLogs();
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
            await Task.Delay(100); // Simulación muy corta, solo para asegurar que se completa
        }
    }
}
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection; // Necesario para GetService

namespace SkinHunterLauncher.ViewModels
{
    public partial class WelcomeViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;

        public WelcomeViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            Title = "Skin-Hunter - Welcome"; // REBRANDED
        }

        [RelayCommand]
        private async Task SignIn()
        {
            await _mainViewModel.NavigateTo<SignInViewModel>();
        }

        [RelayCommand]
        private void Buy()
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://skinhunterv2.vercel.app") { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error opening BUY link: {ex.Message}");
            }
        }
    }
}
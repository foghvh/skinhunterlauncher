using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection;


namespace SkinHunterLauncher.ViewModels
{
    public partial class SignInViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;

        // ELIMINADO LicenseKey
        // [ObservableProperty]
        // private string? _licenseKey;

        [ObservableProperty]
        private bool _rememberMe;

        public SignInViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            Title = "Skin-Hunter - Sign In";
        }

        [RelayCommand]
        private async Task SignInWithDiscord() // NUEVO COMANDO
        {
            // IsLoading = true; // El indicador global de LauncherMainViewModel debería cubrir esto

            // Placeholder para la lógica de OAuth2 de Discord
            Debug.WriteLine("Attempting to sign in with Discord...");

            // Simular el proceso de autenticación
            await Task.Delay(1500);

            bool isAuthenticated = true; // Simular éxito por ahora

            if (isAuthenticated)
            {
                // Si 'Remember me' está marcado, guardarías los tokens de forma segura
                if (RememberMe)
                {
                    Debug.WriteLine("Remember me is checked. (Placeholder for saving tokens)");
                }
                await _mainViewModel.NavigateTo<LoadingViewModel>();
            }
            else
            {
                // Mostrar algún mensaje de error si la autenticación falla
                Debug.WriteLine("Discord authentication failed. (Placeholder)");
                // IsLoading = false; // Asegurarse de que el indicador de carga se desactive
            }
        }

        // ELIMINADOS Comandos Submit y DontHaveKey
    }
}
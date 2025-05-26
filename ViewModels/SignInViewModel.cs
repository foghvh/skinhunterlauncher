using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SkinHunterLauncher.Services;

namespace SkinHunterLauncher.ViewModels
{
    public partial class SignInViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;
        private readonly AuthService _authService;
        private readonly CurrentUserSessionService _sessionService; // AÑADIDO

        [ObservableProperty]
        private string? _username;

        private string? _password;
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        [ObservableProperty]
        private bool _rememberMe;

        public SignInViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            _authService = serviceProvider.GetRequiredService<AuthService>();
            _sessionService = serviceProvider.GetRequiredService<CurrentUserSessionService>(); // AÑADIDO
            Title = "Skin-Hunter - Sign In";
            LoadRememberedUser();
        }

        private void LoadRememberedUser()
        {
            // Si el CurrentUser ya está en la sesión (cargado en App.xaml.cs), usarlo
            if (_sessionService.IsUserLoggedIn && _sessionService.CurrentUser != null)
            {
                Username = _sessionService.CurrentUser.Login;
                RememberMe = true;
                Debug.WriteLine($"User {_sessionService.CurrentUser.Login} pre-filled from current session.");
            }
            else // Si no, intentar cargar desde settings (como antes)
            {
                var (token, rememberedUsername) = _authService.GetRememberedUser();
                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(rememberedUsername))
                {
                    Username = rememberedUsername;
                    RememberMe = true;
                    Debug.WriteLine($"Username {Username} pre-filled from remembered settings (token needs validation on login).");
                }
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            var (success, token, userData, errorMessage) = await _authService.LoginAsync(Username, Password);

            if (success && token != null && userData != null)
            {
                _sessionService.SetCurrentUser(userData, token); // GUARDAR EN SESIÓN

                Debug.WriteLine($"Login successful for {userData.Login}. Token: {token.Substring(0, 10)}...");
                if (RememberMe)
                {
                    _authService.RememberUser(token, userData.Login!);
                }
                else
                {
                    _authService.ClearRememberedUser();
                }
                await _mainViewModel.NavigateTo<LoadingViewModel>();
            }
            else
            {
                MessageBox.Show(errorMessage ?? "Login failed due to an unknown error.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsLoading = false;
        }

        [RelayCommand]
        private void Register()
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://skinhunterv2.vercel.app") { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error opening registration link: {ex.Message}");
                MessageBox.Show($"Could not open registration page: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
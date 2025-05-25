using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SkinHunterLauncher.ViewModels
{
    public partial class LauncherMainViewModel : LauncherBaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private LauncherBaseViewModel? _currentViewModel;

        public LauncherMainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Title = "Skin-Hunter"; // REBRANDED - Main window title
        }

        public override async Task InitializeAsync(object? parameter = null)
        {
            if (CurrentViewModel == null) // Solo navega si no hay ya una vista actual
            {
                await NavigateTo<WelcomeViewModel>();
            }
        }

        public async Task NavigateTo<TViewModel>(object? parameter = null) where TViewModel : LauncherBaseViewModel
        {
            if (CurrentViewModel is IDisposable disposableOld)
            {
                disposableOld.Dispose();
            }

            var newViewModel = _serviceProvider.GetService(typeof(TViewModel)) as LauncherBaseViewModel;
            if (newViewModel != null)
            {
                IsLoading = true;
                CurrentViewModel = newViewModel;
                if (Application.Current.MainWindow != null) // Actualiza el título de la ventana real
                {
                    Application.Current.MainWindow.Title = CurrentViewModel.Title;
                }
                await CurrentViewModel.InitializeAsync(parameter);
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private void MinimizeApplication()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }
    }
}
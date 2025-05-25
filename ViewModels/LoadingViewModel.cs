using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection; // Necesario para GetService

namespace SkinHunterLauncher.ViewModels
{
    public partial class LoadingViewModel : LauncherBaseViewModel
    {
        private readonly LauncherMainViewModel _mainViewModel;
        public LoadingViewModel(IServiceProvider serviceProvider)
        {
            _mainViewModel = serviceProvider.GetRequiredService<LauncherMainViewModel>();
            Title = "Skin-Hunter - Loading"; // REBRANDED
        }

        public override async Task InitializeAsync(object? parameter = null)
        {
            await Task.Delay(1500); // Reducido para pruebas más rápidas
            await _mainViewModel.NavigateTo<MainLauncherViewModel>();
        }
    }
}
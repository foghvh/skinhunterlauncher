using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace SkinHunterLauncher.ViewModels
{
    public abstract partial class LauncherBaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        [ObservableProperty]
        private string _title = "Skin-Hunter"; // REBRANDED

        public virtual Task InitializeAsync(object? parameter = null)
        {
            return Task.CompletedTask;
        }
    }
}
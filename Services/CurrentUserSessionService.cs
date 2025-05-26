using SkinHunterLauncher.Models;
using CommunityToolkit.Mvvm.ComponentModel; // Para ObservableObject

namespace SkinHunterLauncher.Services
{
    public partial class CurrentUserSessionService : ObservableObject
    {
        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string? _sessionToken;

        public bool IsUserLoggedIn => CurrentUser != null && !string.IsNullOrEmpty(SessionToken);

        public void SetCurrentUser(User user, string token)
        {
            CurrentUser = user;
            SessionToken = token;
            // Notificar que las propiedades relacionadas con el usuario pueden haber cambiado globalmente
            OnPropertyChanged(nameof(IsUserLoggedIn));
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
            SessionToken = null;
            OnPropertyChanged(nameof(IsUserLoggedIn));
        }
    }
}
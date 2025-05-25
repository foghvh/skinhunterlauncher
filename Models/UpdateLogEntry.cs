using CommunityToolkit.Mvvm.ComponentModel; // Necesitas este using si ObservableObject viene de aquí
using System.Collections.Generic;


namespace SkinHunterLauncher.Models
{
    public class UpdateLogEntry : ObservableObject // Asegúrate que ObservableObject está disponible
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private List<string> _changes = [];
        public List<string> Changes
        {
            get => _changes;
            set => SetProperty(ref _changes, value);
        }
    }
}
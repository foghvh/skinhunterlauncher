using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkinHunterLauncher.Models
{
    public class SupabaseUpdateLogEntry
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("changes")]
        public List<string>? Changes { get; set; }
    }

    // Clase contenedora si tu JSON es una lista de estos objetos
    public class SupabaseUpdateLogList : ObservableObject
    {
        private List<SupabaseUpdateLogEntry> _logs = new List<SupabaseUpdateLogEntry>();
        public List<SupabaseUpdateLogEntry> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }
    }
}
using Postgrest.Models;
using Postgrest.Attributes;
using System;

namespace SkinHunterLauncher.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)] // 'false' porque es probable que tu int4 sea auto-incremental (identity) o gestionado por secuencia
        public int Id { get; set; } // CAMBIADO DE Guid A int

        [Column("email")]
        public string? Email { get; set; }

        [Column("password")]
        public string? PasswordHash { get; set; }

        [Column("login")]
        public string? Login { get; set; }

        [Column("fichasporskin")]
        public int Credits { get; set; } // Asumiendo que 'numeric' en Supabase mapea bien a 'int' o 'decimal' en C#. 'int' es más simple si no necesitas decimales. Si necesitas decimales, usa 'decimal'.

        [Column("escomprador")]
        public bool IsBuyer { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Models.DTOs
{
    public class LoginRequestDTO
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; } // NOTE: For demo only - NOT encrypted

    }
}

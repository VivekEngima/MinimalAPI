using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Models.DTOs
{
    public class RegistrationRequestDTO
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Password { get; set; } // NOTE: For demo only - NOT encrypted

        public string Role { get; set; } = SD.Role_Customer;
    }
}

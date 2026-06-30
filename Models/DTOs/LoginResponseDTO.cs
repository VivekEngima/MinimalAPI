namespace MinimalAPI.Models.DTOs
{
    public class LoginResponseDTO
    {
        public required string Email { get; set; }
        public required string Token { get; set; }

    }
}

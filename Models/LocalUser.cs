namespace MinimalAPI.Models
{
    public class LocalUser
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; } // NOTE: For demo only - NOT encrypted

        public string Role { get; set; } = SD.Role_Customer;
    }
}

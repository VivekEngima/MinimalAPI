namespace MinimalAPI.Models.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(30, ErrorMessage = "Category name cannot exceed 30 characters.")]
        public required string Name { get; set; }
        public DateTime AddedDate { get; set; }
    }
}

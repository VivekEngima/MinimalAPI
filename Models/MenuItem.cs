using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalAPI.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Store the filename/path of uploaded image
        public string? Image { get; set; }

        // Foreign Key to Category
        public int CategoryId { get; set; }

        // Navigation Property
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
    }
}

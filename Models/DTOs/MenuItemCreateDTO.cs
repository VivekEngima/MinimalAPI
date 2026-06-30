using MinimalAPI.Validation;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Models.DTOs
{
    public class MenuItemCreateDTO
    {
        [Required(ErrorMessage = "Menu item name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }
        [ValidateCategoryId]
        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        // Image file will be sent as IFormFile
    }
}

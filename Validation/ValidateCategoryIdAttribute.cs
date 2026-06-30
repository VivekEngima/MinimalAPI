using MinimalAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Validation
{
    public class ValidateCategoryIdAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not int categoryId)
            {
                return new ValidationResult("Category ID must be a valid integer.");
            }

            var db = validationContext.GetService(typeof(ApplicationDBContext)) as ApplicationDBContext;

            if (db == null)
            {
                return new ValidationResult("Unable to validate Category ID.");
            }

            var categoryExists = db.Categories.Any(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return new ValidationResult("The specified Category ID does not exist.");
            }

            return ValidationResult.Success;
        }
    }
}

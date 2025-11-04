using System;
using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.Validators
{
    /// <summary>
    /// Custom validation attribute để kiểm tra GUID không phải là giá trị rỗng (Guid.Empty).
    /// Hỗ trợ cả Guid và Guid? (nullable).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class GuidNotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            Guid guidValue;
            
            if (value is Guid guid)
            {
                guidValue = guid;
            }
            else
            {
                return new ValidationResult(
                    ErrorMessage ?? $"{validationContext.DisplayName} must be a valid GUID."
                );
            }

            if (guidValue == Guid.Empty)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"{validationContext.DisplayName} must not be an empty GUID."
                );
            }

            return ValidationResult.Success;
        }
    }
}
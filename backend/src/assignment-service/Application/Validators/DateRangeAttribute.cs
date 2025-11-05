using System.ComponentModel.DataAnnotations;

namespace AssignmentService.Application.Validators;

public class DateRangeAttribute : ValidationAttribute
{
    private readonly string _startDateProperty;
    
    public DateRangeAttribute(string startDateProperty)
    {
        _startDateProperty = startDateProperty;
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var endDate = value as DateTime?;
        if (endDate == null) return ValidationResult.Success;
        
        var startDateProperty = validationContext.ObjectType.GetProperty(_startDateProperty);
        if (startDateProperty == null)
            return new ValidationResult($"Property {_startDateProperty} not found");
        
        var startDate = startDateProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
        if (startDate == null) return ValidationResult.Success;
        
        if (startDate >= endDate)
            return new ValidationResult("End time must be after start time");
        
        return ValidationResult.Success;
    }
}
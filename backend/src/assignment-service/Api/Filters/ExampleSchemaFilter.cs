using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AssignmentService.Api.Filters;

/// <summary>
/// Custom schema filter to add examples to Swagger documentation
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == null) return;

        // Add examples for ErrorResponse classes
        if (context.Type.Name == "ErrorResponse" ||
            context.Type.Name == "UnauthorizedErrorResponse" ||
            context.Type.Name == "ForbiddenErrorResponse" ||
            context.Type.Name == "ValidationErrorResponse")
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["success"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false),
                ["message"] = new Microsoft.OpenApi.Any.OpenApiString("An error occurred while processing your request"),
                ["errors"] = new Microsoft.OpenApi.Any.OpenApiArray
                {
                    new Microsoft.OpenApi.Any.OpenApiString("Validation failed"),
                    new Microsoft.OpenApi.Any.OpenApiString("Required field is missing")
                }
            };
            return;
        }


        // Add example for BestSubmissionDto
        if (context.Type.Name == "BestSubmissionDto")
        {
            schema.Example = new OpenApiObject
            {
                ["submissionId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["problemId"] = new OpenApiString("9b844423-5de9-41fb-b425-1c50a9c7a188"),
                ["status"] = new OpenApiString("NOT_STARTED"),
                ["startedAt"] = new OpenApiString("2025-10-25T08:38:39.785Z"),
                ["submittedAt"] = new OpenApiString("2025-10-25T08:38:39.785Z"),
                ["score"] = new OpenApiInteger(0),
                ["maxScore"] = new OpenApiInteger(0),
                ["solutionCode"] = new OpenApiString("string"),
                ["teacherFeedback"] = new OpenApiString("string"),
                ["attemptCount"] = new OpenApiInteger(0),
                ["totalTestCases"] = new OpenApiInteger(100),
                ["passedTestCases"] = new OpenApiInteger(50),
                ["executionTime"] = new OpenApiLong(0),
                ["memoryUsed"] = new OpenApiLong(0)
            };
            return;
        }

        // Add examples for enums
        if (context.Type.IsEnum)
        {
            var enumValues = Enum.GetNames(context.Type);
            schema.Enum = enumValues.Select(name => new OpenApiString(name)).Cast<IOpenApiAny>().ToList();

            // Add example for the first enum value
            if (enumValues.Length > 0)
            {
                schema.Example = new OpenApiString(enumValues[0]);
            }
        }

        // Add examples for Guid properties
        if (context.Type == typeof(Guid))
        {
            schema.Example = new OpenApiString("11111111-1111-1111-1111-111111111111");
        }

        // Add examples for DateTime properties
        if (context.Type == typeof(DateTime))
        {
            schema.Example = new OpenApiString("2024-01-15T10:30:00Z");
        }

        // Add examples for string properties with specific patterns
        if (context.Type == typeof(string))
        {
            var property = context.MemberInfo as PropertyInfo;
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute?.Name != null)
                {
                    switch (displayAttribute.Name.ToLower())
                    {
                        case "code":
                            schema.Example = new OpenApiString("P001");
                            break;
                        case "title":
                            schema.Example = new OpenApiString("Two Sum");
                            break;
                        case "slug":
                            schema.Example = new OpenApiString("two-sum");
                            break;
                    }
                }
            }
        }

        // Add examples for numeric properties
        if (context.Type == typeof(int))
        {
            var property = context.MemberInfo as PropertyInfo;
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute?.Name != null)
                {
                    switch (displayAttribute.Name.ToLower())
                    {
                        case "timelimitms":
                            schema.Example = new OpenApiInteger(1000);
                            break;
                        case "memorylimitkb":
                            schema.Example = new OpenApiInteger(262144);
                            break;
                        case "sourcelimitkb":
                            schema.Example = new OpenApiInteger(65536);
                            break;
                        case "stacklimitkb":
                            schema.Example = new OpenApiInteger(8192);
                            break;
                    }
                }
            }
        }
    }
}

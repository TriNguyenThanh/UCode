using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Application.DTOs.Requests;

namespace UserService.Api.Swagger;

/// <summary>
/// Schema filter để thêm examples cho Swagger UI
/// </summary>
public class SwaggerExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        // Thêm examples cho các DTOs phổ biến
        switch (type.Name)
        {
            case nameof(CreateStudentRequest):
                schema.Example = CreateStudentExample();
                break;
            
            case nameof(CreateTeacherRequest):
                schema.Example = CreateTeacherExample();
                break;
            
            case nameof(CreateClassRequest):
                schema.Example = CreateClassExample();
                break;
            
            case nameof(UpdateUserRequest):
                schema.Example = UpdateUserExample();
                break;
            
            case nameof(UpdateClassRequest):
                schema.Example = UpdateClassExample();
                break;
            
            case nameof(AddStudentsToClassRequest):
                schema.Example = AddStudentsToClassExample();
                break;
            
            case nameof(ResetPasswordRequest):
                schema.Example = ResetPasswordExample();
                break;
            
            case nameof(RequestPasswordResetRequest):
                schema.Example = RequestPasswordResetExample();
                break;
            
            case nameof(ChangePasswordRequest):
                schema.Example = ChangePasswordExample();
                break;
            
            case nameof(LoginRequest):
                schema.Example = LoginExample();
                break;
            
            case nameof(LogoutRequest):
                schema.Example = LogoutExample();
                break;
            
            case nameof(RefreshTokenRequest):
                schema.Example = RefreshTokenExample();
                break;
        }
    }

    private static Microsoft.OpenApi.Any.OpenApiObject CreateStudentExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["studentId"] = new Microsoft.OpenApi.Any.OpenApiString("SV001"),
            ["username"] = new Microsoft.OpenApi.Any.OpenApiString("student01"),
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("student01@ucode.io.vn"),
            ["password"] = new Microsoft.OpenApi.Any.OpenApiString("Student@123"),
            ["fullName"] = new Microsoft.OpenApi.Any.OpenApiString("Nguyễn Văn An"),
            ["major"] = new Microsoft.OpenApi.Any.OpenApiString("Khoa học máy tính"),
            ["enrollmentYear"] = new Microsoft.OpenApi.Any.OpenApiInteger(2023),
            ["classYear"] = new Microsoft.OpenApi.Any.OpenApiInteger(2)
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject CreateTeacherExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["employeeId"] = new Microsoft.OpenApi.Any.OpenApiString("GV001"),
            ["username"] = new Microsoft.OpenApi.Any.OpenApiString("teacher01"),
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("teacher01@ucode.io.vn"),
            ["password"] = new Microsoft.OpenApi.Any.OpenApiString("Teacher@123"),
            ["fullName"] = new Microsoft.OpenApi.Any.OpenApiString("Nguyễn Văn Giáo"),
            ["department"] = new Microsoft.OpenApi.Any.OpenApiString("Khoa Công Nghệ Thông Tin"),
            ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Giảng viên"),
            ["phone"] = new Microsoft.OpenApi.Any.OpenApiString("0912345678")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject CreateClassExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["name"] = new Microsoft.OpenApi.Any.OpenApiString("Lập trình C# cơ bản"),
            ["description"] = new Microsoft.OpenApi.Any.OpenApiString("Khóa học về lập trình C# cho người mới bắt đầu"),
            ["teacherId"] = new Microsoft.OpenApi.Any.OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["classCode"] = new Microsoft.OpenApi.Any.OpenApiString("CLS2401")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject UpdateUserExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["userId"] = new Microsoft.OpenApi.Any.OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("newemail@ucode.io.vn"),
            ["fullName"] = new Microsoft.OpenApi.Any.OpenApiString("Nguyễn Văn An (Updated)"),
            ["phone"] = new Microsoft.OpenApi.Any.OpenApiString("0987654321"),
            ["major"] = new Microsoft.OpenApi.Any.OpenApiString("Công nghệ thông tin"),
            ["classYear"] = new Microsoft.OpenApi.Any.OpenApiInteger(3),
            ["department"] = new Microsoft.OpenApi.Any.OpenApiString("Khoa CNTT"),
            ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Phó Giáo sư")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject UpdateClassExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["classId"] = new Microsoft.OpenApi.Any.OpenApiString("22222222-2222-2222-2222-222222222222"),
            ["name"] = new Microsoft.OpenApi.Any.OpenApiString("Lập trình C# nâng cao"),
            ["description"] = new Microsoft.OpenApi.Any.OpenApiString("Khóa học nâng cao về lập trình C#"),
            ["teacherId"] = new Microsoft.OpenApi.Any.OpenApiString("33333333-3333-3333-3333-333333333333"),
            ["isActive"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject AddStudentsToClassExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["classId"] = new Microsoft.OpenApi.Any.OpenApiString("22222222-2222-2222-2222-222222222222"),
            ["studentIds"] = new Microsoft.OpenApi.Any.OpenApiArray
            {
                new Microsoft.OpenApi.Any.OpenApiString("44444444-4444-4444-4444-444444444444"),
                new Microsoft.OpenApi.Any.OpenApiString("55555555-5555-5555-5555-555555555555")
            }
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject ResetPasswordExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("student01@ucode.io.vn"),
            ["otp"] = new Microsoft.OpenApi.Any.OpenApiString("123456"),
            ["newPassword"] = new Microsoft.OpenApi.Any.OpenApiString("NewPassword@123")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject RequestPasswordResetExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["email"] = new Microsoft.OpenApi.Any.OpenApiString("student01@ucode.io.vn")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject ChangePasswordExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["currentPassword"] = new Microsoft.OpenApi.Any.OpenApiString("OldPassword@123"),
            ["newPassword"] = new Microsoft.OpenApi.Any.OpenApiString("NewPassword@123")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject LoginExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["emailOrUsername"] = new Microsoft.OpenApi.Any.OpenApiString("student01@ucode.io.vn"),
            ["password"] = new Microsoft.OpenApi.Any.OpenApiString("Student@123"),
            ["rememberMe"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false)
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject LogoutExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["refreshToken"] = new Microsoft.OpenApi.Any.OpenApiString("your-refresh-token-here")
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject RefreshTokenExample()
    {
        return new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["refreshToken"] = new Microsoft.OpenApi.Any.OpenApiString("your-refresh-token-here")
        };
    }
}
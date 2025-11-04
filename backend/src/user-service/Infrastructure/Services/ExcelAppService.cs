using OfficeOpenXml;
using UserService.Application.DTOs.Requests;
using UserService.Application.Interfaces.Services;
using UserService.Application.DTOs.Common;

namespace UserService.Infrastructure.Services;

public class ExcelAppService : IExcelService
{
    public async Task<List<CreateStudentRequest>> ImportStudentsFromExcelAsync(Stream fileStream)
    {
        var students = new List<CreateStudentRequest>();

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null)
                throw new ApiException("Excel file is empty or invalid");

            // Expected columns: StudentCode, Username, Email, Password, FullName, Major, EnrollmentYear, ClassYear
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // Start from row 2 (skip header)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var studentId = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var username = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var email = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var fullName = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                    var major = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                    var enrollmentYearStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                    var classYearStr = worksheet.Cells[row, 8].Value?.ToString()?.Trim();

                    // Skip if any required field is empty
                    if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(username) || 
                        string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                        string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(major) ||
                        string.IsNullOrEmpty(enrollmentYearStr) || string.IsNullOrEmpty(classYearStr))
                        continue;

                    if (!int.TryParse(enrollmentYearStr, out int enrollmentYear))
                        continue;

                    if (!int.TryParse(classYearStr, out int classYear))
                        continue;

                    students.Add(new CreateStudentRequest
                    {
                        StudentCode = studentId,
                        Username = username,
                        Email = email,
                        Password = password,
                        FullName = fullName,
                        Major = major,
                        EnrollmentYear = enrollmentYear,
                        ClassYear = classYear
                    });
                }
                catch
                {
                    // Skip invalid rows
                    continue;
                }
            }

            return students;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Failed to import Excel file: {ex.Message}", 500);
        }
    }

    public async Task<byte[]> ExportStudentsToExcelAsync(List<string> studentIds)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            // Headers
            worksheet.Cells[1, 1].Value = "Student ID";
            worksheet.Cells[1, 2].Value = "Username";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Full Name";
            worksheet.Cells[1, 5].Value = "Major";
            worksheet.Cells[1, 6].Value = "Enrollment Year";
            worksheet.Cells[1, 7].Value = "Class Year";
            worksheet.Cells[1, 8].Value = "Status";

            // Style header
            using (var range = worksheet.Cells[1, 1, 1, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Note: In a real implementation, you would fetch student data here
            // For now, this is a placeholder structure
            
            worksheet.Cells.AutoFitColumns();

            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            throw new ApiException($"Failed to export Excel file: {ex.Message}", 500);
        }
    }
}


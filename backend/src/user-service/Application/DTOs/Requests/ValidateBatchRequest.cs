using System.Collections.Generic;

namespace UCode.UserService.Application.DTOs.Requests;

public class ValidateBatchRequest
{
    public List<string> Identifiers { get; set; } = new(); // MSSV or Email
    public string? ClassId { get; set; } // Optional: để check duplicate trong lớp
}

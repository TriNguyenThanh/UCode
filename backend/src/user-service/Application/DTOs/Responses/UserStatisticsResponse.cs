namespace UserService.Application.DTOs.Responses;

public class UserStatisticsResponse
{
    public int TotalUsers { get; set; }
    public int Teachers { get; set; }
    public int Students { get; set; }
    public int Admins { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}

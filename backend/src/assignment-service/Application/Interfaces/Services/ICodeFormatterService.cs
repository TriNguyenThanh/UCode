namespace AssignmentService.Application.Interfaces.Services
{
    public interface ICodeFormatterService
    {
        public Task<string> FormatCode(string code, string language);
    }
}
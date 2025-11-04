namespace AssignmentService.Domain.Enums
{
    public enum SubmissionStatus
    {
        Pending = 1,
        Running = 2,
        Passed = 3,
        Failed = 4
    }

    public enum TestcaseStatus
    {
        Passed = 0,
        TimeLimitExceeded = 1,
        MemoryLimitExceeded = 2,
        RuntimeError = 3,
        InternalError = 4,
        WrongAnswer = 5,
        CompilationError = 6,
        Skipped = 7
    }

    public enum LanguageEnum
    {
        cpp = 1,
        python = 2
    }
}

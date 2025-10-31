namespace AssignmentService.Domain.Enums
{
    public enum SubmissionStatus
    {
        Pending = 1,
        Running = 2,
        SamplePass = 3,
        Done = 4,
        Failed = 5
    }

    public enum TestcaseStatus
    {
        Pending = 1,
        Running = 2,
        Passed = 3,
        TimeLimitExceeded = 4,
        MemoryLimitExceeded = 5,
        RuntimeError = 6,
        InternalError = 7,
        WrongAnswer = 8,
        CompilationError = 9
    }

    public enum Language
    {
        cpp = 1,
        python = 2
    }
}

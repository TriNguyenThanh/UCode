namespace AssignmentService.Domain.Enums;

public enum AssignmentProblemSubmissionStatus
{
    NOT_STARTED = 0,
    IN_PROGRESS = 1,
    SUBMITTED = 2,
    GRADED = 3,
    LATE_SUBMITTED = 4,
    ACCEPTED = 5,
    PARTIAL_ACCEPTED = 6,
    WRONG_ANSWER = 7
}

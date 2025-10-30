namespace ProblemService.Domain.Enums;

/// <summary>
/// Enum representing the difficulty level of a problem
/// Values are stored in database as strings: "EASY", "MEDIUM", "HARD"
/// </summary>
public enum Difficulty
{
    /// <summary>
    /// Easy difficulty - suitable for beginners
    /// </summary>
    EASY,
    
    /// <summary>
    /// Medium difficulty - intermediate level problems
    /// </summary>
    MEDIUM,
    
    /// <summary>
    /// Hard difficulty - advanced problems
    /// </summary>
    HARD
}